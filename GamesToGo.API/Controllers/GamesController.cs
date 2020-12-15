using Ionic.Zip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using GamesToGo.API.Extensions;
using GamesToGo.API.Models;
using GamesToGo.API.Models.File;

namespace GamesToGo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : UserAwareController
    {
        public GamesController(GamesToGoContext context) : base(context)
        {
        }

        // GET: api/Games/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Game>> GetGame(int id)
        {
            var game = await Context.Game.FindAsync(id);

            if (game == null)
            {
                return NotFound();
            }

            return game;
        }

        // PUT: api/Games/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutGame(int id, Game game)
        {
            if (id != game.Id)
            {
                return BadRequest();
            }

            Context.Entry(game).State = EntityState.Modified;

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GameExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // DELETE: api/Games/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<Game>> DeleteGame(int id)
        {
            var game = await Context.Game.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            if (LoggedUser.Id != game.Creator.Id)
            {
                return BadRequest();
            }
            
            Context.Game.Remove(game);
            await Context.SaveChangesAsync();
            System.IO.File.Delete($"Games/{game.Hash}");
            return Ok();
        }

        [HttpPost("UploadFile")]
        [Authorize]
        public async Task<ActionResult> UploadFile([FromForm] FileZip f)
        {
            Directory.CreateDirectory("App_Data");
            Directory.CreateDirectory("Games");
            Game game;
            string gameID = f.ID;
            string name = f.Name;
            string description = f.description;
            string minP = f.minP;
            string maxP = f.maxP;
            string image = f.imageName;
            string le = f.LastEdited;
            var file = f.File;
            var filePath = Path.Combine("App_Data", f.FileName);
            await Task.Run(() =>
            {
                if (file.Length > 0)
                {
                    using var fileStream = file.OpenReadStream();
                    using ZipFile zip = ZipFile.Read(fileStream);
                    foreach (ZipEntry e in zip)
                    {
                        e.Extract(@$"App_Data/{f.FileName.Replace(".zip", "")}");
                    }

                }
            });

            foreach (var inFile in Directory.GetFiles(filePath.Replace(".zip", "")))
            {
                string fileHash = (await System.IO.File.ReadAllBytesAsync(inFile)).SHA1();
                if (fileHash == Path.GetFileName(inFile))
                {
                    await Task.Run(() =>
                    {

                        if (!System.IO.File.Exists($"Games/{Path.GetFileName(inFile)}"))
                        {
                            System.IO.File.Move(inFile, $"Games/{Path.GetFileName(inFile)}");
                            System.IO.File.Delete(inFile);
                        }
                        else
                        {
                            System.IO.File.Delete(inFile);
                        }
                    });
                }
                else
                {
                    return BadRequest();
                }
            }
            Directory.Delete(filePath.Replace(".zip", ""));
            if (int.Parse(gameID) == -1 || (game = await Context.Game.FindAsync(int.Parse(gameID))) == null)
            {
                game = new Game { Creator = await Context.User.FindAsync(LoggedUser.Id) };
                await Context.Game.AddAsync(game);
            }
            
            game.Name = name;
            game.Hash = f.FileName.Replace(".zip", "");
            game.Description = description;
            game.Minplayers = int.Parse(minP);
            game.Maxplayers = int.Parse(maxP);
            game.Image = image;
            game.LastEdited = le;

            game.Status = 3;

            await Context.SaveChangesAsync();
            
            return Ok(new { ID = game.Id, game.Status });
        }

        [HttpGet("DownloadProject/{id}")]
        [Authorize]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var game = await Context.Game.FindAsync(id);
            string gameFile = $"Games/{game.Hash}";


            using ZipFile zip = new ZipFile();
            var stream = new MemoryStream();
            if (System.IO.File.Exists(gameFile))
            {
                await Task.Run(() =>
                {
                    zip.AddFile(gameFile, "");
                    string[] lines = System.IO.File.ReadAllLines(gameFile);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string[] info = lines[i].Split('=');
                        if (info[0] == "Files")
                        {
                            for (int j = 0; j < Int32.Parse(info[1]); j++)
                            {
                                zip.AddFile($"Games/{lines[i + j + 1]}", "");
                            }
                            break;
                        }
                    }
                    zip.Save(stream);
                });
            }
            else
                return NotFound();
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "application/octet-stream", game.Hash + ".zip");
        }

        [HttpGet("DownloadFile/{hash}")]
        public async Task<IActionResult> DownloadSpecificFile(string hash)
        {
            string gFile = $"Games/{hash}";
            var stream = new MemoryStream();
            if (System.IO.File.Exists(gFile))
            {
                await using FileStream fs = System.IO.File.OpenRead(gFile);
                await Task.Run(() => fs.CopyTo(stream));
            }
            else
                return NotFound();
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "application/octet-stream", hash + ".zip");
        }

        [HttpGet("AllGames")]
        [Authorize]
        public async Task<ActionResult<List<Game>>> GetGames()
        {
            List<Game> i;
            i = await Context.Game.Where(x => x.Creator.Id == LoggedUser.Id).ToListAsync();
            return i;
        }

        [HttpGet("UserPublishedGames")]
        [Authorize]
        public async Task<ActionResult<List<Game>>> GetPublishedGames()
        {
            List<Game> g;
            g = await Context.Game.Where(x => x.Creator.Id == LoggedUser.Id && x.Status == 3).ToListAsync();
            return g;
        }

        [HttpGet("AllPublishedGames")]
        [Authorize]
        public async Task<ActionResult<List<Game>>> GetAllPublishedGames()
        { 
            List<Game> g;
            g = await Context.Game.Where(x => x.Status == 3).ToListAsync();
            return g;
        }

        private bool GameExists(int id)
        {
            return Context.Game.Any(e => e.Id == id);
        }
    }
}
