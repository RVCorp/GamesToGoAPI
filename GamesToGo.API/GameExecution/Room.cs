﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using GamesToGo.API.Extensions;
using GamesToGo.API.Models;
using Newtonsoft.Json;

namespace GamesToGo.API.GameExecution
{
    public class Room
    {
        private static int latestCreatedRoom;

        [JsonIgnore]
        public readonly object Lock = new object();

        private readonly Dictionary<int, Token> blueprintTokens;
        private readonly Dictionary<int, Card> blueprintCards;
        private readonly CircularList<ActionParameter> blueprintTurns;
        private readonly List<ActionParameter> blueprintPreparationTurn;
        private readonly List<ActionParameter> blueprintVictoryConditions;

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

        public IReadOnlyList<Board> Boards { get; }

        private IReadOnlyDictionary<int, Tile> CurrentTiles => new Dictionary<int, Tile>(Boards
            .SelectMany(b => b.Tiles)
            .Select(t => new KeyValuePair<int, Tile>(t.TypeID, t)));

        private IReadOnlyDictionary<int, Card> CurrentCards => new Dictionary<int, Card>(CurrentTiles.Values
            .SelectMany(t => t.Cards)
            .Concat(Players.SelectMany(p => p.Tile.Cards))
            .Select(c => new KeyValuePair<int, Card>(c.ID, c)));

        private int latestCardID;

        private IReadOnlyDictionary<int, Token> CurrentTokens => new Dictionary<int, Token>(CurrentCards.Values
            .SelectMany(c => c.Tokens)
            .Concat(CurrentTiles.Values
                .Concat(Players.Select(p => p.Tile))
                .SelectMany(tile => tile.Tokens))
            .Select(token => new KeyValuePair<int, Token>(token.ID, token)));

        private int latestTokenID;

        [JsonIgnore]
        private readonly Random roomRNG = new Random();


        [JsonIgnore]
        private DateTime? timeStarted;

        public bool HasStarted
        {
            get => timeStarted.HasValue;
            private set
            {
                if (!value || HasStarted)
                    return;

                ExecutePreparationTurn();

                timeStarted = DateTime.Now;
            }
        }
    

        public double TimeElapsed => timeStarted == null ? 0 : (timeEnded - timeStarted)?.TotalMilliseconds ?? (DateTime.Now - timeStarted).Value.TotalMilliseconds;

        [JsonIgnore]
        private DateTime? timeEnded;

        public bool HasEnded => timeEnded.HasValue;

        [JsonIgnore]
        private List<int> winningPlayersIndexes;

        public List<int> WinningPlayersIndexes
        {
            get => winningPlayersIndexes;
            set
            {
                if (HasEnded || value == null)
                    return;
                timeEnded = DateTime.Now;

                winningPlayersIndexes = value;
            }
        }

        public bool Errored
        {
            get => HasEnded && winningPlayersIndexes == null;
            private set
            {
                if (value)
                    timeEnded = DateTime.Now;
            }
        }

        private Room(User user, Game game, GameParser parser)
        {
            ID = ++latestCreatedRoom;

            Game = game;
            
            Boards = parser.Boards;
            
            blueprintTurns = new CircularList<ActionParameter>(parser.Turns);
            blueprintCards = new Dictionary<int, Card>(parser.Cards.Select(c => new KeyValuePair<int, Card>(c.TypeID, c)));
            blueprintTokens = new Dictionary<int, Token>(parser.Tokens.Select(t => new KeyValuePair<int, Token>(t.TypeID, t)));
            blueprintPreparationTurn = parser.PreparationParameters;
            blueprintVictoryConditions = parser.VictoryConditions;

            lock (Lock)
                Players = new Player[Game.Maxplayers];

            JoinUser(user);

            Owner = Players[0];
        }

        public static async Task<(Room, ParsingError)> OpenRoom(User user, Game game)
        {
            if (user == null || game == null)
                return (null, ParsingError.Null);

            var gamePath = $"Games/{game.Hash}";

            if (!File.Exists(gamePath))
                return (null, ParsingError.NoFile);
            
            if (game.Hash != (await File.ReadAllBytesAsync(gamePath)).SHA1())
                return (null, ParsingError.WrongHash);
            
            var gameLines = await File.ReadAllLinesAsync(gamePath);

            var parser = new GameParser();
            var status = parser.Parse(gameLines);
            return status == ParsingError.Ok
                ? (new Room(user, game, parser), status)
                : (null, status);
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

                    if (Owner.BackingUser.Id != user.Id || HasEnded)
                        return true;
                    
                    for (int j = 0; j < Players.Length; j++)
                    {
                        if (Players[j] == null)
                            continue;

                        Players[j].BackingUser.Room = null;
                        Players[j] = null;
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

        public bool InteractUser(int idInteraction, User user)
        {
            lock (Lock)
            {
                if (HasEnded)
                    return false;
                int indexOfPlayer = UserActionArgument.Arguments[Array.IndexOf(UserActionArgument.Type.InnerReturnTypes(),
                    ArgumentReturnType.SinglePlayer)].Result[0];
                if (Players[indexOfPlayer].BackingUser.Id != user.Id)
                    return false;
                
                UserActionArgument.Result[0] = idInteraction;
            }

            return true;
        }

        private readonly PriorityQueue<ActionParameter> actionQueue = new PriorityQueue<ActionParameter>();

        private ActionParameter currentAction;

        public ArgumentParameter UserActionArgument { get; set; }

        public void Execute(bool activateEvents = true)
        {
            lock(Lock)
            {
                if (HasEnded)
                    return;
                // An action was interrupted because we needed help from a user
                // See if the user has interacted and continue if so
                if (currentAction != null)
                {
                    PrepareAction(activateEvents);

                    // We still need some help from user
                    // Wait until next iteration
                    if (currentAction != null)
                        return;

                    if (ApplyWinningConditions())
                        return;
                }

                // First try to execute existing items in the queue
                if (actionQueue.TryDequeue(out var actionToClone))
                {
                    currentAction = actionToClone.Clone();
                    PrepareAction(activateEvents);
                }
                // If there are no items in queue, do something from the turns
                else if (blueprintTurns.MoveNext())
                {
                    currentAction = blueprintTurns.Current.Clone();
                    PrepareAction(activateEvents);
                }
                // ABORT! The turns are empty somehow???
                // If we get to here, something went horribly wrong in GameParser.Parse()  !!
                else
                {
                    BailExecution(new InvalidOperationException($"Room {ID} entered an invalid state"));
                }

                ApplyWinningConditions();
            }
        }

        /// <summary>
        /// Computes victory conditions and applies them to their respective users.
        /// If any are applied ends the game and assigns <see cref="WinningPlayersIndexes"/>.
        /// </summary>
        /// <returns>True if the game ended due to one or more condition returning winning players, false otherwise.</returns>
        private bool ApplyWinningConditions()
        {
            // To finish the cycle, get victory conditions and run all and every single one of them
            if (blueprintVictoryConditions.Count == 0)
                BailExecution(new InvalidOperationException($"Room {ID} was initialized without victory conditions"));

            if (currentAction == null)
            {
                List<int> winners = new List<int>();
                    
                foreach (var victoryCondition in blueprintVictoryConditions)
                {
                    if (!(victoryCondition.Type == ActionType.PlayerWins ^ victoryCondition.Type == ActionType.PlayersWins ))
                        BailExecution(new InvalidOperationException($"Attempted to win a game via an argument which does not denote victory {victoryCondition.Type}"));
                        
                    var usableVictoryCondition = victoryCondition.Clone();

                    var condition = usableVictoryCondition.Conditional ??
                                    usableVictoryCondition.Arguments.First(a =>
                                        a.Type.ReturnType() == ArgumentReturnType.Comparison);

                    if (InterpretConditional(condition) && ReplaceArgument(usableVictoryCondition.Arguments[0], out var result) && result != null && result.Result.Count > 0)
                        winners.AddRange(result.Result);
                }
                    
                if (winners.Count > 0)
                    WinningPlayersIndexes = winners;
            }

            return HasEnded;
        }

        /// <summary>
        /// Excecutes an action set in the <see cref="currentAction"/> variable.
        /// </summary>
        /// <summary>
        /// Will try to excecute with current state of arguments, if successful then will set <see cref="currentAction"/> to null
        /// If not successful, will populate <see cref="UserActionArgument"/> with the argument the client needs to solve to continue 
        /// </summary>
        /// <exception cref="NullReferenceException">Thrown if <see cref="currentAction"/> is null</exception>
        private void PrepareAction(bool activateEvents)
        {
            if (currentAction == null)
                BailExecution(new NullReferenceException($"{nameof(currentAction)} was found to be null, game cannot continue"));

            if (!InterpretConditional(currentAction))
            {
                currentAction = null;
                return;
            }

            for (int i = 0; i < currentAction.Arguments.Count; i++)
            {
                var argumentChanged = ReplaceArgument(currentAction.Arguments[i], out var newArgument);

                if (!argumentChanged)
                    continue;
                
                if (newArgument == null)
                    BailExecution(new InvalidOperationException(
                        $"An argument could not be processed, no further action can take place"));
                
                currentAction.Arguments[i] = newArgument;
            }

            if (currentAction.Arguments.Any(a => a.Type != ArgumentType.DefaultArgument))
                return;

            switch (currentAction.Type)
            {
                case ActionType.AddCardToTileChosenByPlayer:
                case ActionType.AddCardToTile:
                {
                    var tile = CurrentTiles[currentAction.Arguments[1].Result[0]];
                    var card = blueprintCards[currentAction.Arguments[0].Result[0]].CloneEmpty(++latestCardID);
                    tile.Cards.Add(card);
                    
                    break;
                }
                case ActionType.ChangeCardPrivacy:
                {
                    var card = CurrentCards[currentAction.Arguments[0].Result[0]];
                    card.Privacy = (Privacy) currentAction.Arguments[1].Result[0];
                    break;
                }
                case ActionType.GivePlayerATokenTypeFromPlayer:
                {
                    var originPlayerTokens = Players[currentAction.Arguments[2].Result[0]].Tile.TokenDictionary;
                    var destinationPlayerTokens = Players[currentAction.Arguments[1].Result[0]].Tile.TokenDictionary;
                    var tokenType = blueprintTokens[currentAction.Arguments[0].Result[0]].Clone();

                    originPlayerTokens[tokenType.TypeID]--;
                    if (originPlayerTokens[tokenType.TypeID].Count == 0)
                        originPlayerTokens.Remove(tokenType.TypeID);
                    
                    if (destinationPlayerTokens.ContainsKey(tokenType.TypeID))
                    {
                        destinationPlayerTokens[tokenType.TypeID]++;
                    }
                    else
                    {
                        destinationPlayerTokens.Add(tokenType.TypeID, tokenType);
                        tokenType.ID = ++latestTokenID;
                        tokenType.Count++;
                    }
                    
                    break;
                }
                case ActionType.RemoveTokenTypeFromPlayer:
                {
                    var playerTokens = Players[currentAction.Arguments[1].Result[0]].Tile
                        .TokenDictionary;

                    int tokenTypeID = currentAction.Arguments[0].Result[0];
                    playerTokens[tokenTypeID]--;

                    if (playerTokens[tokenTypeID].Count == 0)
                        playerTokens.Remove(tokenTypeID);
                        
                    break;
                }
                case ActionType.MoveCardFromPlayerToTile:
                {
                    int cardTypeID = currentAction.Arguments[0].Result[0];
                    var originTile = Players[currentAction.Arguments[1].Result[0]].Tile;

                    var cardToMove = originTile.Cards.First(c => c.TypeID == cardTypeID);

                    originTile.Cards.Remove(cardToMove);
                    
                    CurrentTiles[currentAction.Arguments[2].Result[0]].Cards.Add(cardToMove);
                    
                    break;
                }
                case ActionType.MoveCardFromPlayerToTileInXPosition:
                {
                    int cardTypeID = currentAction.Arguments[0].Result[0];
                    var originTile = Players[currentAction.Arguments[1].Result[0]].Tile;

                    var cardToMove = originTile.Cards.First(c => c.TypeID == cardTypeID);

                    originTile.Cards.Remove(cardToMove);

                    int insertIndex = currentAction.Arguments[3].Result[0];
                    CurrentTiles[currentAction.Arguments[1].Result[0]].Cards.Insert(insertIndex, cardToMove);
                    
                    break;
                }
                case ActionType.ShuffleTile:
                {
                    var tile = CurrentTiles[currentAction.Arguments[0].Result[0]];
                    
                    int n = tile.Cards.Count;
                    while (n > 1)
                    {
                        n--;
                        int k = roomRNG.Next(n + 1);
                        var value = tile.Cards[k];
                        tile.Cards[k] = tile.Cards[n];
                        tile.Cards[n] = value;
                    }
                    
                    break;
                }
                case ActionType.GivePlayerXCardsFromTileAction:
                {
                    break;
                }
                case ActionType.MoveXCardsFromPlayerToTile:
                {
                    
                    break;
                }
                case ActionType.GiveXCardsAToken:
                {
                    break;
                }
                case ActionType.RemoveTokenTypeFromCards:
                {
                    break;
                }
                case ActionType.GivePlayerATokenType:
                {
                    var playerTokens = Players[currentAction.Arguments[1].Result[0]].Tile.TokenDictionary;
                    var tokenType = blueprintTokens[currentAction.Arguments[0].Result[0]].Clone();
                    
                    if (playerTokens.ContainsKey(tokenType.TypeID))
                        playerTokens[tokenType.TypeID]++;
                    else
                    {
                        playerTokens.Add(tokenType.TypeID, tokenType);
                        tokenType.ID = ++latestTokenID;
                        tokenType.Count++;
                    }
                    break;
                }
                case ActionType.GiveCardFromPlayerToPlayer:
                {
                    break;
                }
                case ActionType.GivePlayerXTokensTypeAction:
                {
                    break;
                }
                case ActionType.MoveCardFromPlayerTileToTile:
                {
                    break;
                }
                case ActionType.AddCardTypeToFirstFreeTileInTileChosenByPlayer:
                {
                    float tileColumn = CurrentTiles[currentAction.Arguments[1].Result[0]].Arrangement.X;
                    var tile = CurrentTiles.Values.Where(t => t.Arrangement.X == tileColumn).OrderBy(t => t.Arrangement.X).First(t => t.Cards.Count == 0);
                    var card = blueprintCards[currentAction.Arguments[0].Result[0]].CloneEmpty(++latestCardID);
                    
                    tile.Cards.Add(card);
                    break;
                }
                default:
                {
                    BailExecution(new ArgumentOutOfRangeException($"A not executable ActionType was parsed ({currentAction.Type})"));
                    break;
                }
            }

            currentAction = null;
        }

        private bool InterpretConditional(ActionParameter action)
        {
            if (action == null)
                BailExecution(new ArgumentNullException($"{nameof(action)}",
                    $"A null action was passed to {nameof(InterpretConditional)}"));
            
            var conditional = action.Conditional;

            return InterpretConditional(conditional);
        }
        
        private bool InterpretConditional(ArgumentParameter conditional)
        {
            if (conditional == null)
                return true;

            if (conditional.Type.ReturnType() != ArgumentReturnType.Comparison)
                BailExecution(new InvalidOperationException($"A conditional can only have a return type of {ArgumentReturnType.Comparison} (received {conditional.Type.ReturnType()}"));

            if (!ReplaceArgument(conditional, out var result) || result == null || result.Type != ArgumentType.DefaultArgument)
                BailExecution(new InvalidOperationException(@$"The conditional of an Action contains user-dependant arguments, this is an error with {nameof(GameParser.Parse)}"));

            return result.Result[0] == 1;
        }

        /// <summary>
        /// Reads an argument and tries to convert it to a version understandable by the executor, recursively with all it's inner arguments
        /// </summary>
        /// <param name="argument">the argument to read</param>
        /// <param name="result"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns>True if result is different from argument, false otherwise</returns>
        private bool ReplaceArgument(in ArgumentParameter argument, out ArgumentParameter result)
        {
            if (argument.Type == ArgumentType.DefaultArgument)
            {
                result = argument;
                return false;
            }

            if (argument.Arguments.Count == 0)
            {
                if (argument.Type.ShouldHaveResult())
                {
                    result = new ArgumentParameter
                    {
                        Type = ArgumentType.DefaultArgument,
                        Result = new List<int> { argument.Result[0] },
                    };

                    return true;
                }
                
                result = null;
                return true;
            }

            bool replacedAtLeastOne = false;
            
            for (int i = 0; i < argument.Arguments.Count; i++)
            {
                bool replacedCurrent = ReplaceArgument(argument.Arguments[i], out var newArgument);
                    
                if (replacedCurrent && newArgument != null)
                    argument.Arguments[i] = newArgument;

                replacedAtLeastOne |= replacedCurrent;
            }

            if (!argument.Arguments.TrueForAll(a => a.Type == ArgumentType.DefaultArgument))
            {
                result = argument.Clone();
                return replacedAtLeastOne;
            }

            switch (argument.Type)
            {
                case ArgumentType.CompareCardTypes:
                {
                    result = comparisionResult(CurrentCards[argument.Arguments[0].Result[0]].TypeID == argument.Arguments[1].Result[0]);
                    return true;
                }
                
                case ArgumentType.PlayerRightOfPlayerWithToken:
                {
                    result = new ArgumentParameter
                    {
                        Type = ArgumentType.DefaultArgument,
                        Result = new List<int>
                        {
                            (argument.Arguments[0].Result[0] + 1) % JoinedPlayers,
                        },
                    }; 
                    return true;
                }
                
                case ArgumentType.PlayerWithToken:
                {
                    int tokenTypeID = argument.Arguments[0].Result[0];
                    result = new ArgumentParameter
                    {
                        Type = ArgumentType.DefaultArgument,
                        Result = new List<int>
                        {
                            Array.FindIndex(Players, p => p.Tile.Tokens.Any(t => t.TypeID == tokenTypeID)),
                        },
                    };
                    return true;
                }
                
                case ArgumentType.ComparePlayerWithTokenHasXTokens:
                {
                    int tokenType = argument.Arguments[2].Result[0];

                    result = comparisionResult(
                        Players[argument.Arguments[0].Result[0]].Tile.Tokens.First(t => t.TypeID == tokenType)
                            .Count == argument.Arguments[1].Result[0]);
                    return true;
                }
                
                case ArgumentType.ComparePlayerWithTokenHasMoreThanXTokens:
                {
                    int tokenType = argument.Arguments[2].Result[0];

                    result = comparisionResult(
                        Players[argument.Arguments[0].Result[0]].Tile.Tokens.First(t => t.TypeID == tokenType)
                            .Count > argument.Arguments[1].Result[0]);
                    return true;
                }
                
                case ArgumentType.ComparePlayerHasNoCardType:
                {
                    int cardType = argument.Arguments[1].Result[0];

                    result = comparisionResult(
                        Players[argument.Arguments[0].Result[0]].Tile.Cards.Any(c => c.TypeID != cardType));
                    return true;
                }
                
                case ArgumentType.ComparePlayerHasCardType:
                {
                    int cardType = argument.Arguments[1].Result[0];

                    result = comparisionResult(
                        Players[argument.Arguments[0].Result[0]].Tile.Cards.Any(c => c.TypeID == cardType));
                    return true;
                }
                
                case ArgumentType.ComparePlayerHasTokenType:
                {
                    int cardType = argument.Arguments[1].Result[0];

                    result = comparisionResult(
                        Players[argument.Arguments[0].Result[0]].Tile.Tokens.Any(c => c.TypeID == cardType));
                    return true;
                }
                
                case ArgumentType.FirstXCardsFromTile:
                {
                    result = new ArgumentParameter
                    {
                        Type = ArgumentType.DefaultArgument,
                        Result = new List<int>(CurrentTiles[argument.Arguments[0].Result[0]].Cards
                            .Take(argument.Arguments[0].Result[0]).Select(c => c.ID)),
                    };
                    
                    return true;
                }
                
                case ArgumentType.PlayerCardsWithToken:
                {
                    int tokenType = argument.Arguments[1].Result[0];
                    var playerCards = Players[argument.Arguments[0].Result[0]].Tile.Cards.Where(c => c.Tokens.Any(t => t.TypeID == tokenType)).Select(c => c.ID);

                    result = new ArgumentParameter
                    {
                        Type = ArgumentType.DefaultArgument,
                        Result = playerCards.ToList(),
                    };
                    
                    return true;
                }
                
                case ArgumentType.CompareDirectionHasXTilesWithCards:
                {
                    var tile = CurrentTiles[argument.Arguments[2].Result[0]];
                    int cardType = argument.Arguments[3].Result[0];
                    int expectedToFind = argument.Arguments[0].Result[0];
                    var tileArrangement = tile.Arrangement;
                    var currentLookingArrangement = tile.Arrangement + Vector2.Zero;
                    Tile currentLookingTile;
                    var direction = (Direction)argument.Arguments[1].Result[0];
                    var tileBoard = Boards.First(b => b.Tiles.Any(t => t.TypeID == tile.TypeID));

                    if (tile.Cards.All(c => c.TypeID != cardType))
                    {
                        result = comparisionResult(false);
                        return true;
                    }

                    int consecutiveFound = 1;
                    
                    MoveInDirection(false);

                    while (consecutiveFound < expectedToFind &&
                           (currentLookingTile = tileBoard[(int) currentLookingArrangement.X, (int) currentLookingArrangement.Y]) != null && currentLookingTile.Cards
                               .Any(c => c.TypeID == cardType))
                    {
                        consecutiveFound++;
                        MoveInDirection(false);
                    }

                    currentLookingArrangement = tileArrangement + Vector2.Zero;
                    
                    MoveInDirection(true);
                    
                    while (consecutiveFound < expectedToFind &&
                           (currentLookingTile = tileBoard[(int) currentLookingArrangement.X, (int) currentLookingArrangement.Y]) != null && currentLookingTile.Cards
                               .Any(c => c.TypeID == cardType))
                    {
                        consecutiveFound++;
                        MoveInDirection(true);
                    }

                    result = comparisionResult(consecutiveFound == expectedToFind);
                    
                    return true;

                    void MoveInDirection(bool invert)
                    {
                        switch (direction)
                        {
                            case Direction.Horizontal:
                                if (invert)
                                    currentLookingArrangement -= Vector2.UnitX;
                                else
                                    currentLookingArrangement += Vector2.UnitX;
                                break;
                            case Direction.Vertical:
                                if (invert)
                                    currentLookingArrangement -= Vector2.UnitY;
                                else
                                    currentLookingArrangement += Vector2.UnitY;
                                break;
                            case Direction.DiagonalTopLeft:
                                if (invert)
                                    currentLookingArrangement -= Vector2.One;
                                else
                                    currentLookingArrangement += Vector2.One;
                                break;
                            case Direction.DiagonalTopRight:
                                if (invert)
                                    currentLookingArrangement += new Vector2(1, -1);
                                else
                                    currentLookingArrangement += new Vector2(-1, 1);
                                break;
                            default:
                            {
                                BailExecution(new ArgumentOutOfRangeException($"A lookup was requested in an impossible direction ({direction})"));
                                break;
                            }
                        }
                    }
                }
                    
                case ArgumentType.PlayerAtXPosition:
                {
                    result = new ArgumentParameter
                    {
                        Type = ArgumentType.DefaultArgument,
                        Result = new List<int>
                        {
                            (argument.Arguments[0].Result[0] - 1) % JoinedPlayers,
                        },
                    };
                    return true;
                }
                
                // Arguments that need the user
                
                case ArgumentType.TileWithNoCardsSelectedByPlayer:
                case ArgumentType.TileSelectedByPlayer:
                case ArgumentType.CardSelectedByPlayer:
                case ArgumentType.PlayerChosenByPlayer:
                {
                    if (UserActionArgument != null)
                    {
                        if (UserActionArgument.Type != argument.Type)
                            BailExecution(new InvalidOperationException($"{nameof(UserActionArgument)} was not null when an argument of type different to it was reached"));

                        if (UserActionArgument.Result[0] != -1)
                        {
                            result = new ArgumentParameter
                            {
                                Type = ArgumentType.DefaultArgument,
                                Result = new List<int>
                                {
                                    UserActionArgument.Result[0],
                                },
                            };

                            UserActionArgument = null;
                            
                            return true;
                        }

                        result = argument;

                        return false;
                    }

                    switch (argument.Type)
                    {
                        case ArgumentType.TileWithNoCardsSelectedByPlayer:
                        {
                            if (CurrentTiles.Values.Any(t => t.Cards.Count == 0))
                                break;
                            WinningPlayersIndexes = new List<int>();
                            result = null;
                            return true;
                        }
                    }

                    UserActionArgument = argument.Clone();
                    
                    UserActionArgument.Result.Add(-1);

                    result = UserActionArgument.Clone();

                    return replacedAtLeastOne;
                }

                default:
                {
                    BailExecution(new ArgumentOutOfRangeException(
                        $"An argument of an unexpected type was passed to {nameof(PrepareAction)}"));
                    result = null;
                    return true;
                }
            }

            ArgumentParameter comparisionResult(bool t) => new ArgumentParameter
            {
                Type = ArgumentType.DefaultArgument,
                Result = new List<int>(1) { t ? 1 : 0 },
            };
        }

        private void ExecutePreparationTurn()
        {
            lock (Lock)
            {
                actionQueue.EnqueueRange(blueprintPreparationTurn);

                while (actionQueue.Count > 0 && !Errored)
                {
                    Execute(false);
                    if (currentAction != null)
                        BailExecution(new InvalidOperationException(
                            @$"The conditional of an Action in the preparation turn contains user-dependant arguments, this is an error with {nameof(GameParser.Parse)}"));
                }
            }
        }

        [DoesNotReturn]
        private void BailExecution(Exception e)
        {
            UserActionArgument = null;
            Errored = true;
            throw e;
        }

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