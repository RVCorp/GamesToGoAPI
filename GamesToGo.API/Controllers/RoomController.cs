﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamesToGo.API.GameExecution;
using GamesToGo.API.Models;
using Microsoft.AspNetCore.Http;

namespace GamesToGo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomController : UserAwareController
    {
        private static readonly List<Room> rooms = new List<Room>();

        public RoomController(GamesToGoContext context) : base(context)
        {
        }

        [HttpPost("CreateRoom")]
        public async Task<ActionResult<Room>> CreateRoom([FromForm] string gameID)
        {
            if (LoggedUser.Room != null)
                return Conflict($"Already joined, leave current room to create another one");
            
            Game game = await Context.Game.FindAsync(int.Parse(gameID));
            
            if (game == null)
                return BadRequest($"Game ID {gameID} not found");
            
            (Room Room, ParsingError Status) cRoom = await Room.OpenRoom(LoggedUser, game);

            if (cRoom.Room == null)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, new
                {
                    status = cRoom.Status.ToString(),
                });
            }
            
            rooms.Add(cRoom.Room);
            return cRoom.Room;
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
        public ActionResult<Room> RoomState()
        {
            if (LoggedUser.Room == null)
                return BadRequest();

            if (!LoggedUser.Room.HasStarted)
                return LoggedUser.Room;
            
            UsersController.ClearInvitationsFor(LoggedUser);

            if (LoggedUser.Room.HasEnded)
                return LoggedUser.Room;
            
            try
            {
                LoggedUser.Room.Execute();
            }
            catch
            {
                // ignored
            }

            return LoggedUser.Room;
        }

        [HttpPost("Interact")]
        public ActionResult InteractWithRoom([FromForm] string resultID)
        {
            if (LoggedUser.Room == null || LoggedUser.Room.HasEnded)
                return BadRequest();
            
            return LoggedUser.Room.InteractUser(int.Parse(resultID), LoggedUser) ? Ok() : BadRequest();
        }

        public static Room GetRoom(int id)
        {
            return rooms.FirstOrDefault(x => x.ID == id);
        }

        public static bool LeaveRoom(User user)
        {
            var roomsJoined = rooms.Where(r => r.Players.Any(p => user.Id == p?.BackingUser.Id)).ToList();
            while(roomsJoined.Any())
            {
                var leaving = roomsJoined.First();
                if (!leaving.LeaveUser(user))
                    return false;
                
                roomsJoined.Remove(leaving);

                if (((RoomPreview) leaving).CurrentPlayers > 0)
                    continue;
                
                UsersController.ClearInvitationsFor(leaving);
                rooms.Remove(leaving);
            }

            return true;
        }

        public static bool JoinRoom(User user, Room room)
        {
            return user.Room == null && !room.HasStarted && room.JoinUser(user);
        }
    }
}