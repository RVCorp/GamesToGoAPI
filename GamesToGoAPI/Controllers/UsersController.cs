using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamesToGoAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GamesToGoAPI.Models.GameSettings;
using Microsoft.Extensions.Configuration;
using GamesToGoAPI.Models.File;
using System.IO;

namespace GamesToGoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class UsersController : ControllerBase
    {
        IConfiguration _config;
        private readonly GamesToGoContext _context;
        public static List<Invitation> invitations = new List<Invitation>();

        public UsersController(IConfiguration config, GamesToGoContext context)
        {
            _config = config;
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserPasswordless>>> GetUser()
        {
            List<UserPasswordless> up = new List<UserPasswordless>();
            UserPasswordless nup;
            foreach(var user in await _context.User.ToListAsync())
            {
                nup = new UserPasswordless(user);
                up.Add(nup);
            }
            return up;
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserPasswordless>> GetUser(int id)
        {
            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }
            UserPasswordless up = new UserPasswordless(user);
            return up;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // POST: api/Users
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<UserPasswordless>> PostUser(User user)
        {
            if(!UserExists(user.Username, user.Email))
            {
                user.UsertypeId = 1;
                _context.User.Add(user);
                await _context.SaveChangesAsync();
                UserPasswordless up = new UserPasswordless(user);
                return CreatedAtAction("GetUser", up);
            }
            return BadRequest("");
        }

        [HttpPost("UploadImage")]
        [Authorize]
        public async Task<ActionResult> UploadImage ([FromForm] ImageFile image)
        {
            Directory.CreateDirectory("UserImages");
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claim = identity.Claims.ToList();
            var userID = claim[3].Value;
            User u = _context.User.Where(u => u.Id == int.Parse(userID)).FirstOrDefault();
            var ifile = image.File;
            var filePath = Path.Combine("UserImages", u.Username + Path.GetExtension(ifile.FileName));
            using (var filestream = new FileStream(filePath, FileMode.Create))
            {
                await ifile.CopyToAsync(filestream);
            }
            u.Image = u.Username + Path.GetExtension(ifile.FileName);
            _context.SaveChanges();
            return Ok(new { status = true, message = "Image Posted Successfully" });
        }

        [HttpGet("DownloadImage/{id}")]
        public async Task<IActionResult> DownloadImage(int id)
        {
            User u = await _context.User.FindAsync(id);
            string iFile = $"UserImages/{u.Image}";
            if (System.IO.File.Exists(iFile))
            {
                var stream = new MemoryStream();
                stream.Write(await System.IO.File.ReadAllBytesAsync(iFile));
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/octet-stream", u.Image);
            }
            else
                return NotFound();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<UserPasswordless>> DeleteUser(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            UserPasswordless up = new UserPasswordless(user);

            return up;
        }

        [HttpPost("SendInvitation")]
        public ActionResult<UserPasswordless> SendInvitation(int idUser, int idRoom)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claim = identity.Claims.ToList();
            var userID = claim[3].Value;
            User user =  _context.User.ToListAsync().Result.Where(x => x.Id == idUser).FirstOrDefault();
            Invitation invitation = new Invitation(int.Parse(userID), user.Id, RoomController.getRoom(idRoom));
            invitations.Add(invitation);
            UserPasswordless up = new UserPasswordless(user);
            return up;
        }

        [HttpPost("Updates")]
        public ActionResult<List<Invitation>> UpdateData()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claim = identity.Claims.ToList();
            var userID = claim[3].Value;
            List<Invitation> i;
            i = invitations.Where(x => x.receiver == int.Parse(userID)).ToList();
            return i;
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }

        private bool UserExists(string username, string email)
        {
            return _context.User.Any(e => e.Username == username || e.Email == email);
        }

    }
}
