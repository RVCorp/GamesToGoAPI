using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GamesToGo.API.GameExecution
{
    public class GameParser
    {
        private int fileVersion;
        
        public List<Token> Tokens { get; } = new List<Token>();
        public List<Card> Cards { get; } = new List<Card>();
        public List<Board> Boards { get; } = new List<Board>();

        public List<ActionParameter> Turns { get; } = new List<ActionParameter>();
        public List<ActionParameter> VictoryConditions { get; } = new List<ActionParameter>();
        public List<ActionParameter> PreparationParameters { get; } = new List<ActionParameter>();

        /// <summary>
        /// Parses a game based on the lines sent, populating the stateful lists in the process.
        /// </summary>
        /// <param name="lines">The lines that make up a game</param>
        /// <returns><see cref="ParsingError.Ok"/> if correct, a value describing the error otherwise</returns>
        public ParsingError Parse(string[] lines)
        {
            var infoLines = new List<string>();
            var objectLines = new List<string>();
            var groupedObjectLines = new List<List<string>>();

            bool isParsingObjects = false;

            if (lines[0] != "[Info]")
                return ParsingError.InfoHeader;

            if (!TryGetVersion(lines[1]))
                return ParsingError.Version;

            foreach (var line in lines[ParametersStartingLine..])
            {
                if (line == "[Objects]")
                {
                    isParsingObjects = true;
                    continue;
                }
                
                if (isParsingObjects)
                    objectLines.Add(line);
                else
                    infoLines.Add(line);
            }

            if (objectLines.Count == 0)
                return ParsingError.ObjectsHeader;

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

            var pendingTiles = new List<Tile>();
            var pendingBoardGroups = new List<(int id, IReadOnlyList<ElementParameter> parameters)>();
            
            // Parse base elements (cards and tokens will be cloned in real time, others are there to stay)

            foreach (var group in groupedObjectLines)
            {
                try
                {
                    int id = GetObjectTypeID(group);
                    var parameters = DivideGroup(group.Skip(1));

                    if (parameters == null)
                        return ParsingError.ParameterGroup;

                    if (!TryParseValidateEnum<ElementType>($"{group.First()[0]}", out var elementType))
                        return ParsingError.Object;
                    
                    switch (elementType)
                    {
                        case ElementType.Token:
                            Token newToken = ParseToken(id, parameters);
                            
                            if (newToken == null)
                                return ParsingError.Token;
                            
                            Tokens.Add(newToken);
                            break;
                        
                        case ElementType.Card:
                            Card newCard = ParseCard(id, parameters);
                            
                            if (newCard == null)
                                return ParsingError.Card;
                            
                            Cards.Add(newCard);
                            break;
                        
                        case ElementType.Tile:
                            Tile newTile = ParseTile(id, parameters);
                            
                            if (newTile == null)
                                return ParsingError.Tile;
                                
                            pendingTiles.Add(newTile);
                            break;
                        
                        case ElementType.Board:
                            pendingBoardGroups.Add((id, parameters));
                            break;
                        
                        default:
                            return ParsingError.UnknownObject;
                    }
                }
                catch
                {
                    return ParsingError.Object;
                }
            }
            
            //Parse general sections

            var infoSectionsResult = ParseInfoSections(DivideGroup(infoLines));
            if (infoSectionsResult != ParsingError.Ok)
                return infoSectionsResult;

            foreach (var pendingBoard in pendingBoardGroups)
            {
                var newBoard = ParseBoard(pendingBoard.id, pendingBoard.parameters, pendingTiles);

                if (newBoard == null)
                    return ParsingError.Board;
                
                Boards.Add(newBoard);
            }

            return ParsingError.Ok;
        }
        
        #region Object Parsing
        
        private int GetObjectTypeID(IReadOnlyList<string> lines)
        {
            string first = lines[0];
            return int.TryParse(first[2..first.IndexOf('|', 2)], out int ret) ? ret : 0;
        }

        private Token ParseToken(int id, IReadOnlyList<ElementParameter> parameters)
        {
            var token = new Token(id);

            foreach (var section in parameters)
            {
                switch (section.Name)
                {
                    case "Privacy":
                        if (!TryParseValidateEnum(section.Value, out Privacy privacy))
                            return null;
                        token.Privacy = privacy;
                        break;
                }
            }
            return token;
        }

        private Card ParseCard(int id, IReadOnlyList<ElementParameter> parameters)
        {
            Card card = new Card(id);
            foreach (var section in parameters)
            {
                switch (section.Name)
                {
                    case "Privacy":
                        if (!TryParseValidateEnum(section.Value, out Privacy privacy))
                            return null;
                        card.Privacy = privacy;
                        break;
                    case "Orient":
                        if (!TryParseValidateEnum(section.Value, out Orientation orientation))
                            return null;
                        card.Orientation = orientation;
                        break;
                    case "Side":
                        if (!TryParseValidateEnum(section.Value, out SideVisible sideVisible))
                            return null;
                        card.SideVisible = sideVisible;
                        break;
                    case "Events":
                        var possibleEvents = DivideEvents(section);
                        if (possibleEvents == null)
                            return null;
                        card.Events.AddRange(possibleEvents);
                        break;
                }
            }
            
            return card;
        }

        private Tile ParseTile(int id, IReadOnlyList<ElementParameter> parameters)
        {
            Tile tile = new Tile(id);
            foreach (var section in parameters)
            {
                switch (section.Name)
                {
                    case "Orient":
                        if (!TryParseValidateEnum(section.Value, out Orientation orientation))
                            return null;
                        tile.Orientation = orientation;
                        break;
                    case "Arrangement":
                    {
                        var vector = DivideVector(section.Value);

                        if (!vector.HasValue)
                            return null;

                        tile.Arrangement = vector.Value;
                        break;
                    }
                    case "Events":
                        var possibleEvents = DivideEvents(section);
                        if (possibleEvents == null)
                            return null;
                        tile.Events.AddRange(possibleEvents);
                        break;
                }
            }

            return tile;
        }

        private Board ParseBoard(int id, IReadOnlyList<ElementParameter> parameters, List<Tile> pendingTiles)
        {
            Board board = new Board(id);
            foreach (var section in parameters)
            {
                switch (section.Name)
                {
                    case "SubElems":
                    {
                        foreach(string line in section.ExtraLines)
                        {
                            if (!int.TryParse(line, out int tileID))
                                return null;

                            var possibleTile = pendingTiles.FirstOrDefault(t => t.TypeID == tileID);
                            if (possibleTile == null)
                                return null;

                            pendingTiles.Remove(possibleTile);

                            board.Tiles.Add(possibleTile);
                        }

                        break;
                    }
                }
            }

            return board;
        }
        
        #endregion

        #region Game-wise Parsing

        private ParsingError ParseInfoSections(IReadOnlyList<ElementParameter> infoSections)
        {
            foreach (var section in infoSections)
            {
                switch (section.Name)
                {
                    case "PreparationTurn":
                    {
                        foreach (string preparationLine in section.ExtraLines)
                        {
                            var possibleAction = DivideAction(preparationLine);
                            if (possibleAction == null)
                                return ParsingError.PreparationTurn;
                            PreparationParameters.Add(possibleAction);
                        }

                        if (PreparationParameters.Count != int.Parse(section.Value))
                            return ParsingError.PreparationTurn;
                        break;
                    }
                    case "VictoryConditions":
                    {
                        foreach (string victoryLine in section.ExtraLines)
                        {
                            var possibleAction = DivideAction(victoryLine);
                            if (possibleAction == null)
                                return ParsingError.VictoryConditions;
                            VictoryConditions.Add(possibleAction);
                        }
                        if (VictoryConditions.Count != int.Parse(section.Value))
                            return ParsingError.VictoryConditions;
                        break;
                    }
                    case "Turns":
                    {
                        foreach (string actionLine in section.ExtraLines)
                        {
                            var possibleAction = DivideAction(actionLine);
                            if (possibleAction == null)
                                return ParsingError.Turns;
                            Turns.Add(possibleAction);
                        }
                        if (Turns.Count != int.Parse(section.Value))
                            return ParsingError.Turns;
                        break;
                    }
                }
            }
            
            return ParsingError.Ok;
        }

        private bool TryGetVersion(string versionLine)
        {
            if (versionLine.StartsWith("Version") && !int.TryParse(versionLine.Split('=')[1], out fileVersion))
                return false;
            return true;
        }
        
        #endregion
        
        #region Assignment Parameters

        private IReadOnlyList<ElementParameter> DivideGroup(IEnumerable<string> group)
        {
            var parameters = new List<ElementParameter>();

            ElementParameter currentParameter = null;

            foreach (var line in group)
            {
                if (LineShouldCreateNewParameter(line, currentParameter?.Name))
                {
                    var parts = line.Split('=');

                    if (parts.Length != 2)
                        return null;
                    
                    if (currentParameter != null) 
                        parameters.Add(currentParameter);

                    currentParameter = new ElementParameter
                    {
                        Name = parts[0],
                        Value = parts[1],
                    };
                }
                else if (!StringIsEmptyNullOrWhitespace(line))
                {
                    currentParameter?.ExtraLines.Add(line);
                }
            }
            
            if(currentParameter != null)
                parameters.Add(currentParameter);
            
            return parameters;
        }

        private bool LineShouldCreateNewParameter(string line, string currentParameterName)
        {
            return fileVersion switch
            {
                0 => line.Contains('=') && currentParameterName != "Images",
                _ => line.Contains('='),
            };
        }
        
        #endregion
        
        #region Parenthesis Parameters
        
        private (int Type, string[] Arguments)? SeparateParenthesisParameter(string parenthesisParameter)
        {
            string[] dividedLine = parenthesisParameter.Split('(', 2);

            if (dividedLine.Length != 2 || !int.TryParse(dividedLine[0], out int id))
                return null;

            var argumentsLine = dividedLine[1][..^1];
            
            int parenthesisOpenCount = 0, parenthesisCloseCount = 0, lastStart = 0;
            var argumentsList = new List<string>();

            for (int i = 0; i < argumentsLine.Length; i++)
            {
                switch (argumentsLine[i])
                {
                    case '(':
                        parenthesisOpenCount++;

                        break;
                    case ')':
                        parenthesisCloseCount++;

                        break;
                    case ',':
                        if (parenthesisOpenCount == parenthesisCloseCount)
                        {
                            if (i != lastStart)
                                argumentsList.Add(argumentsLine.Substring(lastStart, i - lastStart));
                            lastStart = i + 1;
                        }

                        break;
                }
            }

            if (parenthesisOpenCount == parenthesisCloseCount)
                argumentsList.Add(argumentsLine.Substring(lastStart));
            
            return (id, argumentsList.ToArray());
        }
        
        #endregion
        
        #region Events, Actions and Arguments

        //TODO: Divide into 2 functions: One for a group of events, one for a singular event
        private IReadOnlyList<EventParameter> DivideEvents(ElementParameter eventsParameter)
        {
            var events = new List<EventParameter>(int.Parse(eventsParameter.Value));
            var actions = new List<ActionParameter>();

            EventParameter currentEventParameter = null;

            foreach (var line in eventsParameter.ExtraLines)
            {
                if (!line.StartsWith('|'))
                {
                    if (currentEventParameter != null)
                    {
                        events.Add(currentEventParameter);
                        currentEventParameter.Actions = actions;
                    }
                    
                    var parts = line.Split('|');

                    if (parts.Length != 6)
                        return null;

                    actions = new List<ActionParameter>(int.Parse(parts[5]));

                    var eventDescriptors = SeparateParenthesisParameter(parts[1]);

                    if (!eventDescriptors.HasValue)
                        return null;

                    var valuedEventDescriptors = eventDescriptors.Value;
                    
                    var arguments = valuedEventDescriptors.Arguments.Select(DivideArgument).ToList();

                    if (arguments.Any(a => a == null))
                        return null;

                    currentEventParameter = new EventParameter
                    {
                        Type = (EventType)valuedEventDescriptors.Type,
                        Priority = int.Parse(parts[3]), 
                        Conditional = DivideArgument(parts[4]),
                        Arguments = arguments,
                    };
                }
                else
                {
                    if (actions.Capacity == actions.Count)
                        return null;

                    var possibleAction = DivideAction(line);

                    if (possibleAction == null)
                        return null;
                    
                    actions.Add(possibleAction);
                }
            }

            if (currentEventParameter != null)
            {
                currentEventParameter.Actions = actions;
                events.Add(currentEventParameter);
            }

            return events;
        }
        
        private ArgumentParameter DivideArgument(string argumentLine)
        {
            if (StringIsEmptyNullOrWhitespace(argumentLine) || argumentLine == "null")
                return null;

            // Server side all parsing should be strict, so we only return valid arguments iff everything nested is valid
            // This saves us some pain in the ass whenever any room is requested to be open with potentially invalid game files

            var argumentDescriptors = SeparateParenthesisParameter(argumentLine);

            if (!argumentDescriptors.HasValue)
                return null;

            var valuedArgumentDescriptors = argumentDescriptors.Value;

            var argumentType = (ArgumentType)valuedArgumentDescriptors.Type;

            if (argumentType.ShouldHaveResult())
            {
                if (valuedArgumentDescriptors.Arguments.Length != 1 ||
                    !int.TryParse(valuedArgumentDescriptors.Arguments[0], out int result))
                    return null;

                return new ArgumentParameter
                {
                    Type = argumentType,
                    Result = new List<int>(1) { result },
                };
            }

            var arguments = valuedArgumentDescriptors.Arguments.Select(DivideArgument).ToList();

            if (arguments.Any(a => a == null))
                return null;

            var argument = new ArgumentParameter
            {
                Type = argumentType,
                Result = null,
                Arguments = arguments,
            };
            
            return argument.Arguments.Any(arg => arg == null) ? null : argument;
        }

        private ActionParameter DivideAction(string actionLine)
        {
            if (!actionLine.StartsWith('|'))
                return null;

            string[] parts = actionLine[1..].Split('|');

            if (parts.Length != 2)
                return null;

            var actionDescriptors = SeparateParenthesisParameter(parts[0]);

            if (!actionDescriptors.HasValue)
                return null;

            var valuedActionDescriptors = actionDescriptors.Value;

            var arguments = valuedActionDescriptors.Arguments.Select(DivideArgument).ToList();

            if (arguments.Any(a => a == null))
                return null;
                
            return new ActionParameter
            {
                Type = (ActionType)valuedActionDescriptors.Type,
                Conditional = DivideArgument(parts[1]),
                Arguments = arguments,
            };
        }
        
        #endregion
        
        #region Helping Functions
        
        private bool TryParseValidateEnum<TEnum>(string sectionValue, out TEnum output) where TEnum : struct, Enum
        {
            bool success = typeof(TEnum) switch
            {
                _ => Enum.TryParse(sectionValue, out output),
            };

            return success && Enum.IsDefined(typeof(TEnum), output);
        }

        private Vector2? DivideVector(string sectionValue)
        {
            string[] xy = sectionValue.Split('|');
            if (xy.Length != 2)
                return null;
            return new Vector2(float.Parse(xy[0]), float.Parse(xy[1]));
        }
        
        private static bool StringIsEmptyNullOrWhitespace(string s) =>
            string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s);

        private bool HasVersion => fileVersion > 0;

        private int ParametersStartingLine => HasVersion ? 2 : 1;

        #endregion
    }
}