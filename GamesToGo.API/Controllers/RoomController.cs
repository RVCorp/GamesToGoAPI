using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamesToGo.API.GameExecution;
using GamesToGo.API.Models;

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
        public ActionResult<Room> CreateRoom([FromForm] string gameID)
        {
            if (LoggedUser.Room != null)
                return Conflict($"Already joined, leave current room to create another one");
            Game game = Context.Game.Find(int.Parse(gameID));
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

        [HttpPost("JoinRoom")]
        public ActionResult<Room> JoinRoom([FromForm] string id)
        {
            Room jRoom = GetRoom(int.Parse(id));

            if (jRoom == null)
                return NotFound($"No such room");
            
            if (!JoinRoom(LoggedUser, jRoom))
                return Conflict("Room is full or already started");
            
            return jRoom;
        }

        [HttpPost("LeaveRoom")]
        public ActionResult LeaveRoom()
        {
            if (LeaveRoom(LoggedUser))
                return Ok();
            return Conflict($"Haven't joined no room");
        }

        [HttpPost("Ready")]
        public ActionResult ReadyUser()
        {
            if (LoggedUser.Room?.ReadyUser(LoggedUser) ?? false)
                return Ok();
            return Conflict(LoggedUser.Room?.Owner.BackingUser.Id == LoggedUser.Id ? "Room not ready" : "Haven't joined no room");
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

        public static bool LeaveRoom(User user)
        {
            var toLeaveRoom = user?.Room;
            if (toLeaveRoom == null || !(bool) toLeaveRoom?.LeaveUser(user))
                return false;
            if (((RoomPreview) toLeaveRoom).CurrentPlayers == 0)
            {
                UsersController.ClearInvitationsFor(toLeaveRoom);
                rooms.Remove(toLeaveRoom);
            }

            return true;
        }

        public static bool JoinRoom(User user, Room room)
        {
            return user.Room == null && !room.HasStarted && room.JoinUser(user);
        }
    }
}