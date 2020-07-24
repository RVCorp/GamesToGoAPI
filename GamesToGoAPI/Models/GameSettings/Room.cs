﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GamesToGoAPI.Models.GameSettings
{
    public class Room
    {
        public int id { get; set; }
        public Game game { get; set; }
        public List<User> users = new List<User>();
        public Room(int id, User user, Game game)
        {
            this.id = id;
            user.RoomID = id;
            users.Add(user);
            this.game = game;
        }

        public void JoinRoom(User user)
        {
            user.RoomID = this.id;
            users.Add(user);
        }
    }
}