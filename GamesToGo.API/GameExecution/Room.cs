using System.Collections.Generic;
using GamesToGo.API.Models;

namespace GamesToGo.API.GameExecution
{
    public class Room
    {
        public int ID { get; set; }
        public Game Game { get; set; }
        public List<User> users = new List<User>();
        public Room(int id, User user, Game game)
        {
            ID = id;
            user.Room = this;
            users.Add(user);
            Game = game;
        }

        public void JoinRoom(User user)
        {
            user.Room = this;
            users.Add(user);
        }
    }
}