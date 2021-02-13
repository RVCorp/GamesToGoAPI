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

            Parse(File.ReadAllLines($"Games/{Game.Hash}"), ref blueprintTokens, ref blueprintCards,
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
            
            return Parse(gameLines, ref dryTokens, ref dryCards, ref dryBoards);
        }
        
        #region Parsing

        private static int GetObjectTypeID(IReadOnlyList<string> lines)
        {
            string first = lines[0];
            return int.TryParse(first[2..first.IndexOf('|', 2)], out int ret) ? 0 : ret;
        }

        private static Token ParseToken(IReadOnlyList<string> tokenLines)
        {
            return new Token(GetObjectTypeID(tokenLines));
        }

        private static Card ParseCard(IReadOnlyList<string> cardLines)
        {
            Card card = new Card(GetObjectTypeID(cardLines));
            for (int i = 1; i < cardLines.Count; i++)
            {
                string[] splitLine = cardLines[i].Split('=');

                if (splitLine.Length != 2)
                    continue;

                switch (splitLine[0])
                {
                    case "Privacy":
                        card.Privacy = Enum.Parse<Privacy>(splitLine[1]);
                        break;
                    case "Orient":
                        card.Orientation = Enum.Parse<Orientation>(splitLine[1]);
                        break;
                    case "Side":
                        card.SideVisible = Enum.Parse<SideVisible>(splitLine[1]);
                        break;
                }
            }

            return card;
        }

        private static Tile ParseTile(IReadOnlyList<string> tileLines)
        {
            Tile tile = new Tile(GetObjectTypeID(tileLines));
            for (int i = 1; i < tileLines.Count; i++)
            {
                string[] splitLine = tileLines[i].Split('=');

                if (splitLine.Length != 2)
                    continue;

                switch (splitLine[0])
                {
                    case "Orient":
                        tile.Orientation = Enum.Parse<Orientation>(splitLine[1]);
                        break;
                }
            }

            return tile;
        }

        private static Board ParseBoard(IReadOnlyList<string> boardLines, List<Tile> pendingTiles)
        {
            Board board = new Board(GetObjectTypeID(boardLines));
            for (int i = 1; i < boardLines.Count; i++)
            {
                string[] splitLine = boardLines[i].Split('=');

                if (splitLine.Length != 2)
                    continue;

                switch (splitLine[0])
                {
                    case "SubElems":
                    {
                        if (!int.TryParse(splitLine[1], out int tiles))
                            return null;
                        for (int j = i + tiles; j > i; j--)
                        {
                            if (!int.TryParse(boardLines[j], out int tileID))
                                return null;

                            var possibleTile = pendingTiles.FirstOrDefault(t => t.TypeID == tileID);
                            if (possibleTile == null)
                                return null;

                            board.Tiles.Add(possibleTile);
                        }

                        i += tiles;

                        break;
                    }
                }
            }

            return board;
        }

        private static bool ParseInfoLines(IReadOnlyList<string> infoLines)
        {
            return true;
        }

        private static bool Parse(IReadOnlyList<string> lines, ref List<Token> tokens, ref List<Card> cards,
            ref List<Board> boards)
        {
            var infoLines = new List<string>();
            var objectLines = new List<string>();
            var groupedObjectLines = new List<List<string>>();

            bool isParsingObjects = false;
            foreach (var line in lines)
            {
                switch (line)
                {
                    case "[Info]":
                        isParsingObjects = false;
                        break;
                    case "[Objects]":
                        isParsingObjects = true;
                        break;
                    default:
                        if (isParsingObjects)
                            objectLines.Add(line);
                        else
                            infoLines.Add(line);
                        break;
                }
            }

            var objectLineGroup = new List<string>();

            foreach (var line in objectLines)
            {
                if (StringIsEmptyNullOrWhitespace(line) && objectLineGroup.Any())
                {
                    groupedObjectLines.Add(objectLineGroup);
                    objectLineGroup = new List<string>();
                    continue;
                }

                objectLineGroup.Add(line);
            }

            if (objectLineGroup.Any())
                groupedObjectLines.Add(objectLineGroup);

            if (!ParseInfoLines(infoLines))
                return false;

            var pendingTiles = new List<Tile>();
            var pendingBoardGroups = new List<List<string>>();
            int highestID = 0;
            
            // Parse base elements (cards and tokens will be cloned in real time, others are there to stay)

            foreach (var group in groupedObjectLines)
            {
                try
                {
                    switch (Enum.Parse<ElementType>(group.First()[..0]))
                    {
                        case ElementType.Token:
                            Token newToken = ParseToken(group);
                            if (newToken != null)
                                tokens.Add(newToken);
                            else
                                return false;
                            if (newToken.TypeID > highestID)
                                highestID = newToken.TypeID;
                            break;
                        case ElementType.Card:
                            Card newCard = ParseCard(group);
                            if (newCard != null)
                                cards.Add(newCard);
                            else
                                return false;
                            if (newCard.TypeID > highestID)
                                highestID = newCard.TypeID;
                            break;
                        case ElementType.Tile:
                            Tile newTile = ParseTile(group);
                            if (newTile != null)
                                pendingTiles.Add(newTile);
                            else
                                return false;
                            if (newTile.TypeID > highestID)
                                highestID = newTile.TypeID;
                            break;
                        case ElementType.Board:
                            pendingBoardGroups.Add(group);
                            break;
                        default:
                            return false;
                    }
                }
                catch
                {
                    return false;
                }
            }

            foreach (var pendingBoard in pendingBoardGroups)
            {
                Board newBoard = ParseBoard(pendingBoard, pendingTiles);
                if (newBoard != null)
                    boards.Add(newBoard);
                else
                    return false;
                if (newBoard.TypeID > highestID)
                    highestID = newBoard.TypeID;
            }

            return true;
        }
        
        #endregion

        /*ProjectElement parsingElement = null;

        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (line.StartsWith('['))
            {
                isParsingObjects = line.Trim('[', ']') switch
                {
                    "Info" => false,
                    "Objects" => true,
                    _ => isParsingObjects,
                };

                continue;
            }

            if (isParsingObjects)
            {
                if (string.IsNullOrEmpty(line))
                {
                    if (parsingElement != null)
                    {
                        AddElement(parsingElement);
                        parsingElement = null;
                    }
                    continue;
                }

                if (parsingElement == null)
                {
                    var idents = line.Split('|', 3);
                    if (idents.Length != 3)
                        return false;

                    parsingElement = Enum.Parse<ElementType>(idents[0]) switch
                    {
                        ElementType.Token => new Token(),
                        ElementType.Card => new Card(),
                        ElementType.Tile => new Tile(),
                        ElementType.Board => new Board(),
                        _ => null,
                    };

                    if (parsingElement == null)
                        return false;

                    parsingElement.ID = int.Parse(idents[1]);
                    parsingElement.Name.Value = idents[2];
                }
                else
                {
                    var parts = line.Split('=');

                    if (parts.Length != 2)
                        return false;

                    switch (parts[0])
                    {
                        case "SubElems" when parsingElement is IHasElements elementedElement:
                        {
                            int amm = int.Parse(parts[1]);
                            for (int j = i + amm; i < j; i++)
                            {
                                elementedElement.QueueElement(int.Parse(lines[i + 1]));
                            }
                            break;
                        }
                        case "Orient" when parsingElement is IHasOrientation orientedElement:
                        {
                            orientedElement.DefaultOrientation.Value = Enum.Parse<Orientation>(parts[1]);
                            break;
                        }
                        case "Privacy" when parsingElement is IHasPrivacy privacySetElement:
                        {
                            privacySetElement.DefaultPrivacy.Value = Enum.Parse<ElementPrivacy>(parts[1]);
                            break;
                        }
                        case "Position" when parsingElement is IHasPosition position:
                        {
                            var xy = parts[1].Split("|");
                            position.Position.Value = new Vector2(float.Parse(xy[0]), float.Parse(xy[1]));
                            break;
                        }
                        case "Events" when parsingElement is IHasEvents eventedElement:
                        {
                            int amm = int.Parse(parts[1]);
                            for (int j = i + amm; i < j; i++)
                            {
                                var splits = lines[i + 1].Split('|');
                                int type = divideLine(splits[1], out string args);

                                ProjectEvent toBeEvent = Activator.CreateInstance(AvailableEvents[type]) as ProjectEvent;
                                toBeEvent.ID = int.Parse(splits[0]);
                                toBeEvent.Condition.Value = populateArgument(splits[4]);
                                toBeEvent.Name.Value = splits[2];
                                toBeEvent.Priority.Value = int.Parse(splits[3]);

                                var eventArgs = args.Split(',', StringSplitOptions.RemoveEmptyEntries);

                                for (int argIndex = 0; argIndex < eventArgs.Length; argIndex++)
                                {
                                    toBeEvent.Arguments[argIndex].Value = populateArgument(eventArgs[argIndex]);
                                }

                                int actAmm = int.Parse(splits[5]);
                                j += actAmm;

                                for (int k = i + actAmm; i < k; i++)
                                {
                                    var action = lines[i + 2].Split('|');
                                    int actType = divideLine(action[1], out string arguments);

                                    var actionArgs = arguments.Split(',');

                                    EventAction toBeAction = Activator.CreateInstance(AvailableActions[actType]) as EventAction;

                                    if (toBeAction.Condition.Value != null)
                                        return false;

                                    for (int argIndex = 0; argIndex < actionArgs.Length; argIndex++)
                                    {
                                        toBeAction.Arguments[argIndex].Value = populateArgument(actionArgs[argIndex]);
                                    }

                                    try
                                    {
                                        toBeAction.Condition.Value = populateArgument(action[2]);
                                    }
                                    catch (IndexOutOfRangeException)
                                    {
                                        toBeAction.Condition.Value = null;
                                    }

                                    toBeEvent.Actions.Add(toBeAction);
                                }

                                eventedElement.Events.Add(toBeEvent);
                            }
                            break;
                        }
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                var parts = line.Split('=');

                if (parts.Length != 2)
                    return false;

                switch (parts[0])
                {
                    case "VictoryConditions":
                        int vicAmm = int.Parse(parts[1]);
                        for(int j = i + vicAmm; i < j; i++)
                        {
                            VictoryConditions.Add(populateAction(lines[i+1]));
                        }
                        break;
                    case "Turns":
                        int turnAmm = int.Parse(parts[1]);
                        for(int j = i + turnAmm; i < j; i++)
                        {
                            Turns.Add(populateAction(lines[i + 1]));
                        }
                        break;
                    default: 
                }
            }
        }

        if (parsingElement != null)
            AddElement(parsingElement);

        return true;
    }

    /// <summary>
    /// Obtiene el argumento y argumentos anidados para el texto introducido
    /// </summary>
    /// <param name="text">el texto del argumento a analizar</param>
    /// <returns>El argumento con sus argumentos anidados formados</returns>
    private static Argument populateArgument(string text)
    {
        if (string.IsNullOrEmpty(text) || text == "null")
            return null;

        var type = divideLine(text, out string argText);

        if (!(Activator.CreateInstance(AvailableArguments[type]) is Argument toBeArgument))
            return null;

        if (toBeArgument.HasResult)
        {
            if (!string.IsNullOrEmpty(argText))
                toBeArgument.Result = int.Parse(argText);

            return toBeArgument;
        }

        if (string.IsNullOrEmpty(argText))
            return toBeArgument;

        var subArgs = argText.Split(',');

        for (int i = 0; i < subArgs.Length; i++)
        {
            toBeArgument.Arguments[i].Value = populateArgument(subArgs[i]);
        }

        return toBeArgument;
    }

    private static EventAction populateAction(string text)
    {
        var action = text.Split('|');
        int actType = divideLine(action[1], out string arguments);

        var actionArgs = arguments.Split(',');

        if(!(Activator.CreateInstance(AvailableActions[actType]) is EventAction toBeAction))
            return null;

        for (int argIndex = 0; argIndex < actionArgs.Length; argIndex++)
        {
            toBeAction.Arguments[argIndex].Value = populateArgument(actionArgs[argIndex]);
        }

        try
        {
            toBeAction.Condition.Value = populateArgument(action[2]);
        }
        catch (IndexOutOfRangeException)
        {
            toBeAction.Condition.Value = null;
        }

        return toBeAction;
    }

    /// <summary>
    /// Divide una linea de evento, accion o argumento en sus partes
    /// </summary>
    /// <param name="line">La linea a dividir</param>
    /// <param name="arguments">Salida de la linea de argumentos</param>
    /// <returns>ID del evento, argumento o accion, -1 si no se encontró</returns>
    private static int divideLine(string line, out string arguments)
    {
        string args = line.Split('(', 2)[1];
        arguments = args.Substring(0, args.Length - 1);
        return int.Parse(line.Split('(')[0]);
    }*/

        private static bool StringIsEmptyNullOrWhitespace(string s) =>
            string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s);

        public enum ElementType
        {
            Token = 0,
            Card = 1,
            Tile = 2,
            Board = 3,
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
                Player targetPlayer = Players.FirstOrDefault(p => p.BackingUser.Id == user.Id);
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