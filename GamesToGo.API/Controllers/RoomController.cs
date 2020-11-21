using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamesToGo.API.GameExecution;
using GamesToGo.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GamesToGo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomController : UserAwareController
    {
        private static int roomID;
        private static readonly List<Room> rooms = new List<Room>();

        public RoomController(GamesToGoContext context) : base(context)
        {
        }

        [HttpPost("CreateRoom")]
        public async Task<ActionResult<Room>> CreateRoom([FromForm] int gameID)
        {
            Game game = await Context.Game.FindAsync(gameID);
            if (game == null)
                return BadRequest($"Game ID {gameID} not found");
            roomID++;
            Room cRoom = new Room(roomID, LoggedUser, game);
            rooms.Add(cRoom);
            return cRoom;
        }

        [HttpGet("AllRoomsFor/{id}")]
        public ActionResult<IEnumerable<RoomPreview>> RoomsForGameID(int id)
        {
            return rooms.Where(r => r.Game.Id == id).Select(r => (RoomPreview) r).ToList();
        }


        [HttpPost("JoinRoom/{id}")]
        public ActionResult<Room> JoinRoom([FromForm] int id)
        {
            Room jRoom = GetRoom(id);
            
            if (jRoom == null || jRoom.HasStarted || !jRoom.JoinRoom(LoggedUser))
                return BadRequest();
            
            return jRoom;
        }

        [HttpGet("RoomState")]
        public ActionResult<Room> JoinedRoomState()
        {
            if (LoggedUser.Room == null)
                return BadRequest();

            return LoggedUser.Room;
        }

        public static Room GetRoom(int id)
        {
            return rooms.FirstOrDefault(x => x.ID == id);
        }
    }
}