using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GamesToGo.API.GameExecution;
using GamesToGo.API.Models;
using GamesToGo.API.Models.GameSettings;

namespace GamesToGo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomController : ControllerBase
    {
        private readonly GamesToGoContext _context;
        private static int roomID = 0;
        private static readonly List<Room> rooms = new List<Room>();

        public RoomController(GamesToGoContext context)
        {
            _context = context;
        }

        [HttpPost("CreateRoom/{gameID}")]
        public async Task<ActionResult<Room>> CreateRoom(int gameID)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claim = identity.Claims.ToList();
            var userID = claim[3].Value;
            Game game = await _context.Game.FindAsync(gameID);
            roomID++;
            Room cRoom = new Room(roomID, LoginController.OnlineUsers[userID], game);
            rooms.Add(cRoom);
            return cRoom;
        }


        [HttpPost("JoinRoom/{id}")]
        public ActionResult<List<Player>> JoinRoom(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claim = identity.Claims.ToList();
            var userID = claim[3].Value;
            Room jRoom = GetRoom(id);
            
            if (jRoom.HasStarted)
                return BadRequest();
            
            jRoom.JoinRoom(LoginController.OnlineUsers[userID]);
            return jRoom.Players.ToList();
        }

        public static Room GetRoom(int id)
        {
            return rooms.FirstOrDefault(x => x.ID == id);
        }
    }
}