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
        private static readonly Dictionary<string, UserPasswordless> onlineUsers = new Dictionary<string, UserPasswordless>();

        public static UserPasswordless GetOnlineUserForClaims(IEnumerable<Claim> claims, GamesToGoContext context)
        {
            string lookup = claims.Skip(3).First().Value;
            if (onlineUsers.ContainsKey(lookup))
                return onlineUsers[lookup];

            addOnlineUser(context.User.Find(int.Parse(lookup)));
            return onlineUsers[lookup];
        }

        public static UserPasswordless GetOnlineUserForString(string lookup)
        {
            if (onlineUsers.ContainsKey(lookup))
                return onlineUsers[lookup];
            
            return null;
        }

        public LoginController(IConfiguration config, GamesToGoContext context)
        {
            this.config = config;
            this.context = context;
        }

        [HttpGet("OnlineUsers")]
        [Authorize]
        public ActionResult<IEnumerable<UserPasswordless>> GetOnlineUsers()
        {
            if (onlineUsers != null)
            {
                return onlineUsers.Values;
            }
            return BadRequest();
        }

        private static void addOnlineUser(User user)
        {
            if (onlineUsers.ContainsKey(user.Id.ToString())) 
                return;
            UserPasswordless up = new UserPasswordless(user);
            onlineUsers.Add(up.Id.ToString(), up);
        }

        [HttpGet]
        public IActionResult Login(string username, string pass)
        {
            //bool mobileLogin = Request.Headers[HeaderNames.UserAgent].Any(ua => ua == "gtg-app");
            var login = new User
            {
                Username = username,
                Password = pass
            };
            IActionResult response = Unauthorized();

            var user = AuthenticateUser(login);

            if (user != null)
            {
                var tokenStr = GenerateJWT(user);
                response = Ok(new { token = tokenStr, id = user.Id });
                addOnlineUser(user);
            }
            return response;
        }

        private User AuthenticateUser(User login)
        {
            User user = context.User.FirstOrDefault(x => (x.Username == login.Username || x.Email == login.Username) && x.Password == login.Password);
            return user;
        }

        private string GenerateJWT(User userinfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            { 
                new Claim (JwtRegisteredClaimNames.Sub, userinfo.Username),
                new Claim (JwtRegisteredClaimNames.Email, userinfo.Email),
                new Claim (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim (JwtRegisteredClaimNames.NameId, userinfo.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(20),
                signingCredentials: credentials
                );

            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);
            return encodedToken;
        }
    }
}