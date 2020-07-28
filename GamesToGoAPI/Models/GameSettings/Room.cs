using System.Collections.Generic;

namespace GamesToGoAPI.Models.GameSettings
{
    public class Room
    {
        public int ID { get; set; }
        public Game Game { get; set; }
        public List<User> users = new List<User>();
        public Room(int id, User user, Game game)
        {
            ID = id;
            user.RoomID = id;
            users.Add(user);
            Game = game;
        }

        public void JoinRoom(User user)
        {
            user.RoomID = ID;
            users.Add(user);
        }
    }
}
