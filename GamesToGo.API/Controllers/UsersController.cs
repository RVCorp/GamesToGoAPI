using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GamesToGo.API.Models;
using GamesToGo.API.Models.File;

namespace GamesToGo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class UsersController : UserAwareController
    {
        private static List<Invitation> invitations = new List<Invitation>();

        public UsersController(GamesToGoContext context) : base(context)
        {
        }

        // GET: api/Users
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserPasswordless>>> GetUser()
        {
            List<UserPasswordless> up = new List<UserPasswordless>();
            UserPasswordless nup;
            foreach (var user in await Context.User.ToListAsync())
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
            var user = await Context.User.FindAsync(id);

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

            Context.Entry(user).State = EntityState.Modified;

            try
            {
                await Context.SaveChangesAsync();
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
            if (UserExists(user.Username, user.Email)) 
                return BadRequest("");
            user.UsertypeId = 1;
            await Context.User.AddAsync(user);
            await Context.SaveChangesAsync();
            UserPasswordless up = new UserPasswordless(user);
            return CreatedAtAction("GetUser", up);
        }

        [HttpPost("UploadImage")]
        [Authorize]
        public async Task<ActionResult> UploadImage([FromForm] ImageFile image)
        {
            Directory.CreateDirectory("UserImages");
            var ifile = image.File;
            var filePath = Path.Combine("UserImages", LoggedUser.Username + Path.GetExtension(ifile.FileName));
            await using (var filestream = new FileStream(filePath, FileMode.Create))
            {
                await ifile.CopyToAsync(filestream);
            }
            LoggedUser.Image = LoggedUser.Username + Path.GetExtension(ifile.FileName);
            await Context.SaveChangesAsync();
            return Ok(new { status = true, message = "Image Posted Successfully" });
        }

        [HttpGet("DownloadImage/{id}")]
        public async Task<IActionResult> DownloadImage(int id)
        {
            User u = await Context.User.FindAsync(id);
            string iFile = $"UserImages/{u.Image}";
            if (System.IO.File.Exists(iFile))
            {
                var stream = new MemoryStream();
                stream.Write(await System.IO.File.ReadAllBytesAsync(iFile));
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/octet-stream", u.Image);
            }

            return NotFound();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<UserPasswordless>> DeleteUser(int id)
        {
            var user = await Context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            Context.User.Remove(user);
            await Context.SaveChangesAsync();

            UserPasswordless up = new UserPasswordless(user);

            return up;
        }

        [HttpPost("SendInvitation")]
        [Authorize]
        public ActionResult SendInvitation([FromForm] int receiver)
        {
            var userReceiver = LoginController.GetOnlineUserForInt(receiver);
            if (userReceiver == null)
                return BadRequest();
                
            var invitation = new Invitation(LoggedUser, userReceiver, LoggedUser.Room);
            invitations.Add(invitation);
            return Ok();
        }

        [HttpGet("Invitations")]
        [Authorize]
        public ActionResult<List<Invitation>> GetInvitations()
        {
            return invitations.Where(x => x.Receiver == LoggedUser).ToList();
        }

        private bool UserExists(int id)
        {
            return Context.User.Any(e => e.Id == id);
        }

        private bool UserExists(string username, string email)
        {
            return Context.User.Any(e => e.Username == username || e.Email == email);
        }

    }
}
