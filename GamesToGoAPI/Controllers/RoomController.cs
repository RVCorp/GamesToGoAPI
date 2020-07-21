using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GamesToGoAPI.Models;
using GamesToGoAPI.Models.GameSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GamesToGoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomController : ControllerBase
    {
        private readonly GamesToGoContext _context;
        private int roomID = 0;
        public static List<Room> rooms = new List<Room>();

        public RoomController(GamesToGoContext context)
        {
            _context = context;
        }

        [HttpPost("CreateRoom/Game={gameID}")]
        public async Task<ActionResult<Room>> CreateRoom( int gameID)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claim = identity.Claims.ToList();
            var userID = claim[3].Value;
            Game game = _context.Game.Where(g => g.Id == gameID).FirstOrDefault();
            roomID++;
            Room cRoom = new Room(roomID, _context.User.ToList().Where(x => x.Id == Int32.Parse(userID)).FirstOrDefault(), game);
            rooms.Add(cRoom);
            return cRoom;
        }


        [HttpPost("JoinRoom/{id}")]
        public async Task<ActionResult<List<User>>> JoinRoom(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claim = identity.Claims.ToList();
            var userID = claim[3].Value;
            Room jRoom = getRoom(id);
            jRoom.JoinRoom(_context.User.ToList().Where(x => x.Id == Int32.Parse(userID)).FirstOrDefault());
            return jRoom.users;
        }

        public static Room getRoom(int roomID)
        {
            return rooms.ToList().Where(x => x.id == roomID).FirstOrDefault();
        }
    }
}