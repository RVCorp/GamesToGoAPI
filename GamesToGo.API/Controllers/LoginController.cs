using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using GamesToGo.API.Extensions;
using GamesToGo.API.Models;
//using Microsoft.Net.Http.Headers;

namespace GamesToGo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : UserAwareController
    {
        private readonly IConfiguration config;
        private static readonly object onlineUsersLock = new object();
        private static readonly Dictionary<int, User> onlineUsers = new Dictionary<int, User>();
        public static readonly Thread CheckOfflineThread = new Thread(CheckForOfflineUsers)
        {
            Name = "Offline User Purging",
            IsBackground = true,
        };

        private static void CheckForOfflineUsers()
        {
            lock (onlineUsersLock)
            {
                foreach (var offlineUser in onlineUsers.Where(u => u.Value.LogoutTime <= DateTime.Now))
                {
                    UsersController.ClearInvitationsFor(offlineUser.Value);
                    if (offlineUser.Value.Room != null)
                        RoomController.LeaveRoom(offlineUser.Value);
                    onlineUsers.Remove(offlineUser.Key);
                }
            }

            Thread.Sleep(1000);
        }

        private static User getOnlineUser(int lookup)
        {
            lock (onlineUsersLock)
            {
                if (!onlineUsers.TryGetValue(lookup, out var onlineUser))
                    return null;
                if (onlineUser.LogoutTime <= DateTime.Now)
                {
                    if (onlineUser.Room != null)
                        RoomController.LeaveRoom(onlineUser);
                    onlineUsers.Remove(lookup);
                    return null;
                }

                return onlineUser;
            }
        }

        public static User GetOnlineUserForClaims(IEnumerable<Claim> claims, GamesToGoContext context)
        {
            int userID = int.Parse(claims.ElementAt(3).Value);
            addOnlineUser(context.User.Find(userID));
            return getOnlineUser(userID);
        }

        public static User GetOnlineUserForID(int lookup) => getOnlineUser(lookup);

        public LoginController(IConfiguration config, GamesToGoContext context) : base(context)
        {
            this.config = config;
        }

        [HttpGet("OnlineUsers")]
        [Authorize]
        public ActionResult<IEnumerable<User>> GetOnlineUsers()
        {
            lock (onlineUsersLock)
            {
                if (onlineUsers == null)
                    return BadRequest();
            }

            lock (onlineUsersLock)
            {
                foreach (var offlineUser in onlineUsers.Where(u => u.Value.LogoutTime <= DateTime.Now)
                    .Select(ou => ou.Key))
                    onlineUsers.Remove(offlineUser);
            }

            lock (onlineUsersLock)
                return onlineUsers.Values.Except(new[] {LoggedUser}).ToList();
        }

        private static void addOnlineUser(User user)
        {
            lock (onlineUsersLock)
            {
                if (onlineUsers.TryGetValue(user.Id, out var existingUser))
                {
                    if (existingUser.ManualLogout)
                        return;
                    existingUser.LogoutTime = DateTime.Now.AddMinutes(1);
                    return;
                }
            }
            
            user.LogoutTime = DateTime.Now.AddMinutes(1);
            
            lock (onlineUsersLock)
                onlineUsers.Add(user.Id, user);
        }

        [HttpGet("Logout")]
        [Authorize]
        public IActionResult Logout()
        {
            lock (onlineUsersLock)
            {
                LoggedUser.ManualLogout = true;
                LoggedUser.LogoutTime = DateTime.Now;
            }

            return Ok();
        }

        [HttpGet]
        public IActionResult Login(string username, string pass)
        {
            //bool mobileLogin = Request.Headers[HeaderNames.UserAgent].Any(ua => ua == "gtg-app");

            var user = Context.UserLogin.Where(x => x.User.Username == username || x.Email == username).FirstOrDefault(x => x.Password == pass);

            if (user == null)
                return Unauthorized();
            
            var tokenStr = GenerateJWT(user);
            addOnlineUser(user.User);
            return Ok(new {token = tokenStr, id = user.User.Id});
        }

        private string GenerateJWT(UserLogin userinfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            { 
                new Claim(JwtRegisteredClaimNames.Jti, validString(Guid.NewGuid().ToByteArray().SHA256())),
                new Claim(JwtRegisteredClaimNames.Sub, validString(userinfo.User.Username.SHA256())),
                new Claim(JwtRegisteredClaimNames.Email, validString(userinfo.Email.SHA256())),
                new Claim(JwtRegisteredClaimNames.NameId, userinfo.User.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.AuthTime, validString($"{DateTime.Now:O}".SHA256())), 
            };

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
                );

            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);
            return encodedToken;

            string validString(string s) => $"{s}{DateTime.Now:O}".SHA256();
        }
    }
}