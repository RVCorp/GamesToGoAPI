using Ionic.Zip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GamesToGo.API.Extensions;
using GamesToGo.API.GameExecution;
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
            var game = await Context.Game.Include(g => g.Creator).SingleAsync(g => g.Id == id);
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

            await Context.SaveChangesAsync();

            game.Status = new GameParser().Parse(await System.IO.File.ReadAllLinesAsync($"Games/{game.Hash}")) == ParsingError.Ok ? 3 : 2;
            
            return Ok(new { ID = game.Id, game.Status });
        }

        [HttpGet("GameFiles/{id}")]
        [Authorize]
        public ActionResult<string[]> FindGameFiles(int id)
        {
            var files = GetFilesForGame(id).ToArray();
            if (files.Length == 0)
                return NotFound("Game was not in database");
            return files.ToArray();
        }

        [HttpGet("DownloadProject/{id}")]
        [Authorize]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var files = GetFilesForGame(id).ToArray();
            
            if (files.Length == 0)
                return NotFound();

            using ZipFile zip = new ZipFile();
            var stream = new MemoryStream();
            
            await Task.Run(() =>
            {
                foreach(var file in files)
                    zip.AddFile($"Games/{file}", "");
                
                zip.Save(stream);
            });
            
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "application/octet-stream", files.First() + ".zip");
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
            return File(stream, "application/octet-stream", hash);
        }

        [HttpGet("AllGames")]
        [Authorize]
        public async Task<ActionResult<List<Game>>> GetGames()
        {
            var i = await Context.Game.Where(x => x.Creator.Id == LoggedUser.Id).Include(game => game.Creator).ToListAsync();
            return i;
        }

        [HttpGet("UserPublishedGames")]
        [Authorize]
        public async Task<ActionResult<List<Game>>> GetPublishedGames()
        {
            var g = await Context.Game.Where(x => x.Creator.Id == LoggedUser.Id && x.Status == 3).Include(game => game.Creator).ToListAsync();
            return g;
        }

        [HttpGet("AllPublishedGames")]
        [Authorize]
        public async Task<ActionResult<List<Game>>> GetAllPublishedGames()
        {
            var g = await Context.Game.Where(x => x.Status == 3).Include(game => game.Creator).ToListAsync();
            return g;
        }
        
        /// <summary>
        /// Gets the list of files that a project was uploaded with
        /// </summary>
        /// <param name="id">The ID of the game</param>
        /// <returns>An enumerable of hashes, including the hash of the game itself</returns>
        public IEnumerable<string> GetFilesForGame(int id)
        {
            //Let's find the game in the database first...
            var gameHash = Context.Game.Find(id)?.Hash;
            
            //ABORT! No such game was found in database!
            if(gameHash == null)
                yield break;
            
            var gameFile = $"Games/{gameHash}";

            //ABORT! File was not found on file-system!
            //TODO: What's good to do in this case?
            if (!System.IO.File.Exists(gameFile))
                yield break;

            //Skip until we find the "Files = XX" line
            var skippedLines = System.IO.File.ReadAllLines(gameFile).SkipWhile(l => !l.StartsWith("Files")).ToArray();

            //We have to include the game file too (it is a file after all)
            yield return gameHash;

            //Add the rest of files
            foreach(var file in skippedLines.Skip(1).Take(int.Parse(skippedLines.First().Split('=')[1])).ToArray())
                yield return file;
        }

        private bool GameExists(int id)
        {
            return Context.Game.Any(e => e.Id == id);
        }
    }
}
