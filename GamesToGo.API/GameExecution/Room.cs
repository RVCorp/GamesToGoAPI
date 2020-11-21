using System;
using System.Collections.Generic;
using GamesToGo.API.Models;
using Newtonsoft.Json;

namespace GamesToGo.API.GameExecution
{
    public class Room
    {
        [JsonIgnore]
        public readonly object Lock = new object();

        private int latestBoardID = 1;
        private int latestCardID = 1;
        private int latestTileID = 1;

        public Player Owner { get; }
        public int ID { get; }
        public Game Game { get; }

        public Player[] Players { get; }

        public List<Board> Boards { get; } = new List<Board>(); 
        
        [JsonIgnore]
        private DateTime? timeStarted;

        public bool HasStarted
        {
            get => timeStarted.HasValue;
            set
            {
                if (!value || HasStarted)
                    return;
                timeStarted = DateTime.Now;
            }
        }

        public double TimeElapsed => timeStarted == null ? 0 : (DateTime.Now - timeStarted).Value.TotalMilliseconds;
        
        public Room(int id, UserPasswordless user, Game game)
        {
            ID = id;
            Game = game;
            
            Players = new Player[Game.Maxplayers];
            
            JoinRoom(user);

            Owner = Players[0];

            ParseGame();
        }

        private void ParseGame()
        {
            
        }

        public bool JoinRoom(UserPasswordless user)
        {
            lock(Lock)
            {
                for (int i = 0; i < Players.Length; i++)
                {
                    if (Players[i] != null)
                        continue;

                    Players[i] = new Player(user)
                    {
                        RoomPosition = i,
                    };
                    
                    user.Room = this;
                    
                    return true;
                }

                return false;
            }
        }

        public bool MovePlayer(Player player, int desiredPosition)
        {
            if (Players[desiredPosition] != null)
                return false;
            lock (Lock)
            {
                Players[desiredPosition] = player;
                Players[player.RoomPosition] = null;
                player.RoomPosition = desiredPosition;
            }
            return true;
        }

        public Player PlayerAtIndex(int index) => Players[index % Players.Length];

        public Player PlayerAtIndexAfterPlayer(int index, Player player) =>
            Players[(index + player.RoomPosition) % Players.Length];

        public Player PlayerAtIndexBeforePlayer(int index, Player player)
        {
            int targetIndex = player.RoomPosition - (index % Players.Length);
            return targetIndex < 0 ? Players[Players.Length + targetIndex] : Players[targetIndex];
        }

        public Player PlayerAfter(Player player) => PlayerAtIndexAfterPlayer(1, player);

        public Player PlayerBefore(Player player) => PlayerAtIndexBeforePlayer(1, player);
        
        public static explicit operator RoomPreview(Room r) => new RoomPreview(r);
    }

    public class RoomPreview
    {
        public int ID { get; }
        
        public UserPasswordless Owner { get; }
        
        public int CurrentPlayers { get; }
        
        public Game Game { get; }
        
        public RoomPreview(Room room)
        {
            ID = room.ID;
            Owner = room.Owner.BackingUser;
            Game = room.Game;

            lock (room.Lock)
            {
                foreach (var player in room.Players)
                {
                    if (player != null)
                        CurrentPlayers++;
                }
            }
        }
    }
}