using GamesToGoAPI.Models;
using GamesToGoAPI.Models.File;
using Ionic.Zip;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GamesToGoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class GamesController : ControllerBase
    {
        private readonly GamesToGoContext _context;

        public GamesController(GamesToGoContext context)
        {
            _context = context;
        }

        // GET: api/Games
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Game>>> GetGame()
        {
            return await _context.Game.ToListAsync();
        }

        // GET: api/Games/5
        [HttpGet("{id}")]
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
            else if(up.Id != game.CreatorId)
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
        public async Task<ActionResult> UploadFile([FromForm]FileZip f)
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
            var file = f.File;
            var filePath = Path.Combine("App_Data", file.FileName);
            if (file.Length > 0)
            {
                using (var fileStream = file.OpenReadStream())
                {
                    using (ZipFile zip = ZipFile.Read((fileStream)))
                    {
                        foreach (ZipEntry e in zip)
                        {
                            e.Extract(@$"App_Data/{file.FileName.Replace(".zip", "")}");
                        }
                    }
                }

            }
            foreach (var inFile in Directory.GetFiles(filePath.Replace(".zip", "")))
            {
                if (HashBytes(System.IO.File.ReadAllBytes(inFile)) == Path.GetFileName(inFile))
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
                }
                else
                {
                    return BadRequest();
                }
            }
            Directory.Delete(filePath.Replace(".zip", ""));
            if (Int32.Parse(ID) == -1)
            {
                game = new Game();
                game.Name = name;
                game.Hash = file.FileName.Replace(".zip", "");
                game.Description = description;
                game.Minplayers = Int32.Parse(minP);
                game.Maxplayers = Int32.Parse(maxP);
                game.Image = image;
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                IList<Claim> claim = identity.Claims.ToList();
                var id = claim[3].Value;
                game.Creator = _context.User.Where(u => u.Id == Int32.Parse(id)).FirstOrDefault();
                _context.Game.Add(game);
            }
            else
            {
                game = _context.Game.Where(g => g.Id == Int32.Parse(ID)).FirstOrDefault();
                game.Name = name;
                game.Hash = file.FileName.Replace(".zip", "");
                game.Description = description;
                game.Minplayers = Int32.Parse(minP);
                game.Maxplayers = Int32.Parse(maxP);
                game.Image = image;
            }
            _context.SaveChanges();
            return Ok(new { status = true, ID = game.Id });
        }



        [HttpGet("Download/{id}")]
        public IActionResult DownloadFile(int id)
        {
            string hash = _context.Game.Where(g => g.Id == id).FirstOrDefault().Hash;
            string GFile = $"Games/{hash}";

            using (ZipFile zip = new ZipFile())
            {
                var stream = new MemoryStream();
                if (System.IO.File.Exists(GFile))
                {
                    zip.AddFile(GFile, "");
                    string[] lines = System.IO.File.ReadAllLines(GFile);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string[] info = lines[i].Split('=');
                        if(info[0] == "Files")
                        {
                            for (int j = 0; j < Int32.Parse(info[1]); j++)
                            {
                                zip.AddFile($"Games/{lines[i + j + 1]}","");
                            }
                            break;
                        }
                    }
                    zip.Save(stream);
                }
                else
                    return NotFound();
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/octet-stream", hash + ".zip");
            }
        }

        [HttpGet("AllGames")]
        public async Task<ActionResult<List<Game>>> GetGames()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claim = identity.Claims.ToList();
            var userID = claim[3].Value;
            List<Game> i;
            i = _context.Game.Where(x => x.CreatorId == Int32.Parse(userID)).ToList();
            return i;
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
