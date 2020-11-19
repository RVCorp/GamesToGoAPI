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
using Microsoft.Net.Http.Headers;

namespace GamesToGo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly GamesToGoContext _context;
        public static readonly Dictionary<string, UserPasswordless> OnlineUsers = new Dictionary<string, UserPasswordless>();

        public LoginController(IConfiguration config, GamesToGoContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpGet("OnlineUsers")]
        [Authorize]
        public ActionResult<IEnumerable<UserPasswordless>> GetOnlineUsers()
        {
            if (OnlineUsers != null)
            {
                return OnlineUsers.Values;
            }
            return BadRequest();
        }

        [HttpGet]
        public IActionResult Login(string username, string pass)
        {
            base.Request.Headers[HeaderNames.UserAgent].Any(ua => ua == "");
            User login = new User
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
                User loggedUser = _context.User.Where(u => u.Username == login.Username || u.Email == login.Username).FirstOrDefault();
                if (!OnlineUsers.ContainsKey(loggedUser.Id.ToString()))
                {
                    UserPasswordless up = new UserPasswordless(loggedUser);


                    OnlineUsers.Add(up.Id.ToString(), up);
                }
            }
            return response;
        }

        private User AuthenticateUser(User login)
        {
            User user = _context.User.Where(x => (x.Username == login.Username || x.Email == login.Username) && x.Password == login.Password).FirstOrDefault();
            return user;
        }

        private string GenerateJWT(User userinfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim (JwtRegisteredClaimNames.Sub, userinfo.Username),
                new Claim (JwtRegisteredClaimNames.Email, userinfo.Email),
                new Claim (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim (JwtRegisteredClaimNames.NameId, userinfo.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(20),
                signingCredentials: credentials
                );

            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);
            return encodedToken;
        }
    }
}