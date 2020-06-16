using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GamesToGoAPI.Models.GameSettings
{
    public class Room
    {
        public int id { get; set; }
        public List<User> users = new List<User>();
        public Room(int id, User user)
        {
            this.id = id;
            user.RoomID = id;
            users.Add(user);
        }

        public void JoinRoom(User user)
        {
            user.RoomID = this.id;
            users.Add(user);
        }
    }
}
