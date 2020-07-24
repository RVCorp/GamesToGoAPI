﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GamesToGoAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GamesToGoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _config;
        private readonly GamesToGoContext _context;
        public static List<User> onlineUsers = new List<User>();

        public LoginController(IConfiguration config, GamesToGoContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpPost]
        public ActionResult<List<User>> GetOnlineUsers()
        {
            if(onlineUsers != null)
            {
                return onlineUsers;
            }
            return BadRequest();
        }

        [HttpGet]
        public IActionResult Login(string username, string pass)
        {
            User login = new User();
            login.Username = username;
            login.Password = pass;
            IActionResult response = Unauthorized();

            var user = AuthenticateUser(login);

            if(user != null)
            {
                var tokenStr = GenerateJWT(user);
                response = Ok(new { token = tokenStr, id = user.Id });
                User loggedUser = _context.User.Where(u => u.Username == login.Username || u.Email == login.Email).FirstOrDefault();
                onlineUsers.Add(loggedUser);
            }
            return response;
        }

        private User AuthenticateUser(User login)
        {
            User user = _context.User.ToList().Where(x => x.Username == login.Username || x.Email == login.Username && x.Password == login.Password).FirstOrDefault();
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
                issuer:_config["Jwt:Issuer"],
                audience: _config["Jwt:Issuer"],
                claims,
                expires:DateTime.Now.AddYears(2),
                signingCredentials: credentials
                );

            var encodetoken = new JwtSecurityTokenHandler().WriteToken(token);
            return encodetoken;
        }
    }
}