using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GamesToGo.API.Extensions;
using GamesToGo.API.Models;
using Newtonsoft.Json;

namespace GamesToGo.API.GameExecution
{
    public class Room
    {
        private static int latestCreatedRoom;

        [JsonIgnore] public readonly object Lock = new object();

        private readonly List<Board> blueprintBoards = new List<Board>();
        private readonly List<Token> blueprintTokens = new List<Token>();
        private readonly List<Card> blueprintCards = new List<Card>();

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

        [JsonIgnore] private DateTime? timeStarted;

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

        private Room(User user, Game game)
        {
            ID = ++latestCreatedRoom;

            Game = game;

            lock (Lock)
                Players = new Player[Game.Maxplayers];

            JoinUser(user);

            Owner = Players[0];

            new GameParser().Parse(File.ReadAllLines($"Games/{Game.Hash}"), ref blueprintTokens, ref blueprintCards,
                ref blueprintBoards);
        }

        public static async Task<Room> OpenRoom(User user, Game game)
        {
            if (user == null || game == null)
                return null;

            if (!await ParseDry(game))
                return null;

            return new Room(user, game);
        }

        private static async Task<bool> ParseDry(Game game)
        {
            if (!File.Exists($"Games/{game.Hash}"))
                return false;
            var gameBytes = await File.ReadAllBytesAsync($"Games/{game.Hash}");
            if (gameBytes.SHA1() != game.Hash)
                return false;
            var gameLines = Encoding.UTF8.GetString(gameBytes)
                .Split(new[] {"\n\r", "\r\n", "\n"}, StringSplitOptions.None);
            var dryTokens = new List<Token>();
            var dryCards = new List<Card>();
            var dryBoards = new List<Board>();
            
            return new GameParser().Parse(gameLines, ref dryTokens, ref dryCards, ref dryBoards);
        }

        public bool JoinUser(User user)
        {
            lock (Lock)
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
            lock (Lock)
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
                            if (Players[j] == null)
                                continue;

                            Players[j].BackingUser.Room = null;
                            Players[j] = null;
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
                Player targetPlayer = Players.FirstOrDefault(p => p?.BackingUser.Id == user.Id);
                if (targetPlayer == null)
                    return false;
                if (targetPlayer == Owner)
                {
                    if (JoinedPlayers >= Game.Minplayers &&
                        Players.Except(new[] {Owner}).Where(p => p != null).All(p => p.Ready))
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

    public record RoomPreview
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