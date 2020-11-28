using System;
using System.Collections.Generic;
using System.Linq;
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

        public int JoinedPlayers
        {
            get
            {
                lock (Lock)
                {
                    return Players.Count(p => p != null);
                }
            }
        }

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
        
        public Room(int id, User user, Game game)
        {
            ID = id;
            Game = game;
            
            Players = new Player[Game.Maxplayers];
            
            JoinUser(user);

            Owner = Players[0];

            ParseGame();
        }

        private void ParseGame()
        {
            
        }

        public bool JoinUser(User user)
        {
            lock(Lock)
            {
                if (HasStarted)
                    return false;
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
            lock (Lock)
            {
                if (Players[desiredPosition] != null)
                    return false;
                Players[desiredPosition] = player;
                Players[player.RoomPosition] = null;
                player.RoomPosition = desiredPosition;
            }
            return true;
        }
        
        public bool LeaveUser(User user)
        {
            lock(Lock)
            {
                for (int i = 0; i < Players.Length; i++)
                {
                    if (Players[i] == null)
                        continue;

                    if (Players[i].BackingUser.Id != user.Id) 
                        continue;
                    
                    Players[i] = null;
                    user.Room = null;
                        
                    if (Owner.BackingUser.Id == user.Id)
                    {
                        for (int j = 0; j < Players.Length; j++)
                        {
                            if (Players[i] == null)
                                continue;

                            Players[i].BackingUser.Room = null;
                            Players[i] = null;
                        }
                    }
                        
                    return true;
                }
            }

            return false;
        }
        
        public bool ReadyUser(User user)
        {
            lock (Lock)
            {
                if (HasStarted)
                    return false;
                Player targetPlayer = Players.FirstOrDefault(p => p.BackingUser.Id == user.Id);
                if (targetPlayer == null)
                    return false;
                if (targetPlayer == Owner)
                {
                    if (JoinedPlayers >= Game.Minplayers && Players.Except(new[] { Owner }).Where(p => p != null).All(p => p.Ready))
                        HasStarted = true;
                    else
                        return false;
                }
                targetPlayer.Ready = true;
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
        
        public User Owner { get; }
        
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