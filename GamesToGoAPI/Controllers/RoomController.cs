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

        [HttpPost("CreateRoom")]
        public async Task<ActionResult<Room>> CreateRoom()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claim = identity.Claims.ToList();
            var userID = claim[3].Value;
            roomID++;
            Room cRoom = new Room(roomID, _context.User.ToList().Where(x => x.Id == Int32.Parse(userID)).FirstOrDefault());
            rooms.Add(cRoom);
            return cRoom;
        }


        [HttpPost("JoinRoom/{id}")]
        public async Task<ActionResult<List<User>>> JoinRoom(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claim = identity.Claims.ToList();
            var userID = claim[3].Value;
            Room jRoom = rooms.ToList().Where(x => x.id == id).FirstOrDefault();
            jRoom.JoinRoom(_context.User.ToList().Where(x => x.Id == Int32.Parse(userID)).FirstOrDefault());
            return jRoom.users;
        }

        [HttpPost("UpdateRoom/{id}")]
        public string UpdateRoom(int id)
        {
            if (true)
                return "Petición para actualizar la partida enviada";
            else
                return "No se pudo realizar la petición para actualizar la partida";
        }

        [HttpPost("Updates/{id}")]
        public void CheckUpdates(int id)
        {

        }

        public static Room getRoom(int roomID)
        {
            return rooms.ToList().Where(x => x.id == roomID).FirstOrDefault();
        }
    }
}