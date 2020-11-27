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
using GamesToGo.API.Extensions;
using GamesToGo.API.Models;
//using Microsoft.Net.Http.Headers;

namespace GamesToGo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly GamesToGoContext context;
        private static readonly Dictionary<string, User> onlineUsers = new Dictionary<string, User>();

        private static User getOnlineUser(string lookup, GamesToGoContext logoutTimeUpdate = null)
        {
            if (onlineUsers.TryGetValue(lookup, out var onlineUser))
            {
                if (logoutTimeUpdate != null)
                {
                    onlineUser.LogoutTime = DateTime.Now.AddMinutes(1);
                    logoutTimeUpdate.SaveChangesAsync();
                }

                if (onlineUser.LogoutTime > DateTime.Now) 
                    return onlineUser;

                if (onlineUser.Room != null)
                    RoomController.LeaveRoom(onlineUser);
                onlineUsers.Remove(lookup);
                return null;
            }

            if (logoutTimeUpdate != null)
            {
                addOnlineUser(logoutTimeUpdate.User.Find(int.Parse(lookup)));
                onlineUsers[lookup].LogoutTime = DateTime.Now.AddMinutes(1);
                logoutTimeUpdate.SaveChanges();
                return onlineUsers[lookup];
            }

            return null;
        }

        public static User GetOnlineUserForClaims(IEnumerable<Claim> claims, GamesToGoContext context) =>
            getOnlineUser(claims.ElementAt(3).Value, context);

        public static User GetOnlineUserForString(string lookup) => getOnlineUser(lookup);

        public LoginController(IConfiguration config, GamesToGoContext context)
        {
            this.config = config;
            this.context = context;
        }

        [HttpGet("OnlineUsers")]
        [Authorize]
        public ActionResult<IEnumerable<User>> GetOnlineUsers()
        {
            if (onlineUsers == null) 
                return BadRequest();
            
            foreach (var offlineUser in onlineUsers.Where(u => u.Value.LogoutTime <= DateTime.Now).Select(ou => ou.Key))
                onlineUsers.Remove(offlineUser);
            return onlineUsers.Values;
        }

        private static void addOnlineUser(User user)
        {
            if (onlineUsers.ContainsKey(user.Id.ToString())) 
                return;
            onlineUsers.Add(user.Id.ToString(), user);
        }

        [HttpGet]
        public IActionResult Login(string username, string pass)
        {
            //bool mobileLogin = Request.Headers[HeaderNames.UserAgent].Any(ua => ua == "gtg-app");

            var user = context.UserLogin.Where(x => x.User.Username == username || x.Email == username).FirstOrDefault(x => x.Password == pass);

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