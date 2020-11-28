using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GamesToGo.API.GameExecution;
using GamesToGo.API.Models;
using GamesToGo.API.Models.File;

namespace GamesToGo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : UserAwareController
    {
        private static Dictionary<string, Invitation> invitations = new Dictionary<string, Invitation>();
        private static int latestInvitationID = 1;

        public UsersController(GamesToGoContext context) : base(context)
        {
        }

        // GET: api/Users
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
            return await Context.User.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await Context.User.FindAsync(id);

            if (user == null)
                return NotFound();

            return user;
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
        public async Task<ActionResult<User>> PostUser(UserLogin user)
        {
            if (UserExists(user.User.Username) || UserExists(user.Email))
                return BadRequest("");
            await Context.UserLogin.AddAsync(user);
            await Context.SaveChangesAsync();
            var returnUser = await Context.User.FindAsync(user.User);
            return CreatedAtAction("GetUser", returnUser);
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
            return Ok(new {status = true, message = "Image Posted Successfully"});
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
        public async Task<ActionResult<User>> DeleteUser(int id)
        {
            var user = await Context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            Context.User.Remove(user);
            await Context.SaveChangesAsync();

            return user;
        }

        [HttpPost("SendInvitation")]
        [Authorize]
        public ActionResult SendInvitation([FromForm] string receiver)
        {
            if (!int.TryParse(receiver, out int receiverID))
                return BadRequest("NaN");
            var userReceiver = LoginController.GetOnlineUserForID(receiverID);
            if (userReceiver == null)
                return BadRequest();
            if (LoggedUser.Room == null)
                return Conflict();

            var invitation = new Invitation
            {
                ID = latestInvitationID++,
                Sender = LoggedUser,
                Receiver = userReceiver,
                Room = LoggedUser.Room,
            };
            invitations.Add(invitation.ID.ToString(), invitation);
            return Ok();
        }

        [HttpGet("Invitations")]
        [Authorize]
        public ActionResult<List<Invitation>> GetInvitations()
        {
            return invitations.Values.Where(x => x.Receiver == LoggedUser).ToList();
        }

        [HttpPost("AcceptInvitation")]
        [Authorize]
        public ActionResult<Room> AcceptInvitation([FromForm] string invitationID)
        {
            if (!invitations.ContainsKey(invitationID) || invitations[invitationID].Receiver.Id != LoggedUser.Id)
                return BadRequest();

            var accepted = invitations[invitationID];

            if (LoggedUser.Room != null)
                RoomController.LeaveRoom(LoggedUser);

            if (!RoomController.JoinRoom(LoggedUser, accepted.Room))
                return Conflict();

            invitations.Remove(invitationID);
            return LoggedUser.Room;
        }

        [HttpPost("IgnoreInvitation")]
        [Authorize]
        public IActionResult IgnoreInvitation([FromForm] string invitationID)
        {
            if (!invitations.ContainsKey(invitationID) || invitations[invitationID].Receiver.Id != LoggedUser.Id)
                return BadRequest();

            invitations.Remove(invitationID);
            return Ok();
        }

        [HttpGet("Statistics")]
        [Authorize]
        public async Task<ActionResult<List<NamedStatistic>>> GetUserStatistics()
        {
            ConstructStatistics(LoggedUser, Context);
            return await Context.UserStatistic
                .Where(s => s.User.Id == LoggedUser.Id)
                .Select(s => new NamedStatistic
                {
                    Amount = s.Amount,
                    Name = GetEnumDescription(s.Type),
                }).ToListAsync();
        }

        private static void ConstructStatistics(User user, GamesToGoContext context)
        {
            var existing = context.UserStatistic.Where(s => s.User.Id == user.Id).ToList();
            var expected = Enum.GetValues<UserStatisticType>();
            var existingTypes = existing.Select(s => s.Type).ToList();
            var expectedTypes = expected.ToList();

            for (int i = 0; i < existingTypes.Count; i++)
            {
                if (expectedTypes.All(t => t != existingTypes[i])) 
                    continue;
                
                expectedTypes.Remove(existingTypes[i]);
                existingTypes.Remove(existingTypes[i]);
                i--;
            }

            if (existingTypes.Any() || expectedTypes.Any())
            {
                foreach (var newStat in expectedTypes)
                {
                    context.UserStatistic.Add(new UserStatistic
                    {
                        Type = newStat,
                        User = context.User.Find(user.Id),
                    });
                }

                foreach (var oldStat in existingTypes)
                {
                    context.UserStatistic.Remove(existing.First(s => s.Type == oldStat));
                }
            }

            context.SaveChanges();
        }

        private bool UserExists(int id)
        {
            return Context.User.Any(e => e.Id == id);
        }

        private bool UserExists(string userEmail)
        {
            return Context.UserLogin.Any(e => e.User.Username == userEmail || e.Email == userEmail);
        }

        private static string GetEnumDescription(object enumValue)
        {
            return enumValue.GetType().GetField(enumValue.ToString()!)?
                .GetCustomAttribute<DescriptionAttribute>()?.Description ?? enumValue.ToString();
        }

        public static void ClearInvitationsFor(User user)
        {
            var toRemove = invitations.Where(i => i.Value.Receiver.Id == user.Id || i.Value.Sender.Id == user.Id).Select(i => i.Key);
            foreach (var removable in toRemove)
            {
                invitations.Remove(removable);
            }
        }

        public static void ClearInvitationsFor(Room toLeaveRoom)
        {
            var toRemove = invitations.Where(i => i.Value.Room == toLeaveRoom).Select(i => i.Key);
            foreach (var removable in toRemove)
            {
                invitations.Remove(removable);
            }
        }
    }
}