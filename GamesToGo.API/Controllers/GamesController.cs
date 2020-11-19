using Ionic.Zip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using GamesToGo.API.Models;
using GamesToGo.API.Models.File;

namespace GamesToGo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly GamesToGoContext _context;

        public GamesController(GamesToGoContext context)
        {
            _context = context;
        }

        // GET: api/Games/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Game>> GetGame(int id)
        {
            var game = await _context.Game.FindAsync(id);

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

            _context.Entry(game).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claim = identity.Claims.ToList();
            var userID = claim[3].Value;
            UserPasswordless up = new UserPasswordless(_context.User.Where(u => u.Id == Int32.Parse(userID)).FirstOrDefault());
            var game = await _context.Game.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            else if (up.Id != game.CreatorId)
            {
                return BadRequest();
            }
            else
            {
                _context.Game.Remove(game);
                await _context.SaveChangesAsync();
                System.IO.File.Delete($"Games/{game.Hash}");
                return Ok();
            }
        }

        [HttpPost("UploadFile")]
        [Authorize]
        public async Task<ActionResult> UploadFile([FromForm] FileZip f)
        {
            Directory.CreateDirectory("App_Data");
            Directory.CreateDirectory("Games");
            Game game;
            string ID = f.ID;
            string name = f.Name;
            string description = f.description;
            string minP = f.minP;
            string maxP = f.maxP;
            string image = f.imageName;
            string le = f.LastEdited;
            int status = f.Status;
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
                string fileHash = string.Empty;
                await Task.Run(() => fileHash = HashBytes(System.IO.File.ReadAllBytes(inFile)));
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
            if (int.Parse(ID) == -1)
            {
                game = new Game
                {
                    Name = name,
                    Hash = f.FileName.Replace(".zip", ""),
                    Description = description,
                    Minplayers = int.Parse(minP),
                    Maxplayers = int.Parse(maxP),
                    Image = image,
                    LastEdited = le
                };
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                IList<Claim> claim = identity.Claims.ToList();
                var id = claim[3].Value;
                game.Creator = _context.User.Where(u => u.Id == int.Parse(id)).FirstOrDefault();
                await _context.Game.AddAsync(game);
            }
            else
            {
                game = _context.Game.Where(g => g.Id == int.Parse(ID)).FirstOrDefault();
                game.Name = name;
                game.Hash = f.FileName.Replace(".zip", "");
                game.Description = description;
                game.Minplayers = int.Parse(minP);
                game.Maxplayers = int.Parse(maxP);
                game.Image = image;
                game.LastEdited = le;
                game.Status = status;
            }

            await _context.SaveChangesAsync();
            return Ok(new { ID = game.Id });
        }

        [HttpGet("DownloadProject/{id}")]
        [Authorize]
        public async Task<IActionResult> DownloadFile(int id)
        {
            string hash = _context.Game.Where(g => g.Id == id).FirstOrDefault().Hash;
            string GFile = $"Games/{hash}";


            using ZipFile zip = new ZipFile();
            var stream = new MemoryStream();
            if (System.IO.File.Exists(GFile))
            {
                await Task.Run(() =>
                {
                    zip.AddFile(GFile, "");
                    string[] lines = System.IO.File.ReadAllLines(GFile);
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
            return File(stream, "application/octet-stream", hash + ".zip");
        }

        [HttpGet("DownloadFile/{hash}")]
        public async Task<IActionResult> DownloadSpecificFile(string hash)
        {
            string gFile = $"Games/{hash}";
            var stream = new MemoryStream();
            if (System.IO.File.Exists(gFile))
            {
                using FileStream fs = System.IO.File.OpenRead(gFile);
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
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claim = identity.Claims.ToList();
            var userID = claim[3].Value;
            List<Game> i;
            i = await _context.Game.Where(x => x.CreatorId == int.Parse(userID)).ToListAsync();
            return i;
        }

        [HttpGet("UserPublishedGames")]
        [Authorize]
        public async Task<ActionResult<List<Game>>> GetPublishedGames()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claim = identity.Claims.ToList();
            var userID = claim[3].Value;
            List<Game> g;
            g = _context.Game.Where(x => x.CreatorId == Int32.Parse(userID) && x.Status == 3).ToList();
            return g;
        }

        [HttpGet("AllPublishedGames")]
        [Authorize]
        public async Task<ActionResult<List<Game>>> GetAllPublishedGames()
        { 
            List<Game> g;
            g = _context.Game.Where(x => x.Status == 3).ToList();
            return g;
        }

        private bool GameExists(int id)
        {
            return _context.Game.Any(e => e.Id == id);
        }

        public static string HashBytes(byte[] bytes) //Obtiene SHA1 de una secuencia de bytes
        {
            using SHA1Managed hasher = new SHA1Managed();
            return string.Concat(hasher.ComputeHash(bytes).Select(by => by.ToString("X2")));
        }
    }
}
