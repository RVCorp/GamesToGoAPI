using System;
using System.Collections.Generic;
using System.Linq;

namespace GamesToGo.API.GameExecution
{
    public class GameParser
    {
        private int fileVersion;
        
        public bool Parse(IReadOnlyList<string> lines, ref List<Token> tokens, ref List<Card> cards,
            ref List<Board> boards)
        {
            var infoLines = new List<string>();
            var objectLines = new List<string>();
            var groupedObjectLines = new List<List<string>>();

            bool isParsingObjects = false;
            foreach (var line in lines)
            {
                if (line.StartsWith("Version") && !int.TryParse(line.Split('=')[1], out fileVersion))
                    return false;
                
                isParsingObjects = line switch
                {
                    "[Info]" => false,
                    "[Objects]" => true,
                    _ => isParsingObjects,
                };

                if (line.StartsWith('['))
                    continue;
                
                if (isParsingObjects)
                    objectLines.Add(line);
                else
                    infoLines.Add(line);
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
                        return false;
                    
                    switch (Enum.Parse<ElementType>(group.First()[..0]))
                    {
                        case ElementType.Token:
                            Token newToken = ParseToken(id, parameters);
                            if (newToken != null)
                                tokens.Add(newToken);
                            else
                                return false;
                            break;
                        case ElementType.Card:
                            Card newCard = ParseCard(id, parameters);
                            if (newCard != null)
                                cards.Add(newCard);
                            else
                                return false;
                            break;
                        case ElementType.Tile:
                            Tile newTile = ParseTile(id, parameters);
                            if (newTile != null)
                                pendingTiles.Add(newTile);
                            else
                                return false;
                            break;
                        case ElementType.Board:
                            pendingBoardGroups.Add((id, parameters));
                            break;
                        default:
                            return false;
                    }
                }
                catch
                {
                    return false;
                }
                
                //Parse info lines

                if (!ParseInfoLines(DivideGroup(infoLines)))
                    return false;
            }

            foreach (var pendingBoard in pendingBoardGroups)
            {
                Board newBoard = ParseBoard(pendingBoard.id, pendingBoard.parameters, pendingTiles);
                
                if (newBoard != null)
                    boards.Add(newBoard);
                else
                    return false;
            }

            return true;
        }
        
        private int GetObjectTypeID(IReadOnlyList<string> lines)
        {
            string first = lines[0];
            return int.TryParse(first[2..first.IndexOf('|', 2)], out int ret) ? 0 : ret;
        }

        private Token ParseToken(int id, IReadOnlyList<ElementParameter> parameters)
        {
            var token = new Token(id);

            foreach (var section in parameters)
            {
                switch (section.Name)
                {
                    case "Privacy":
                        token.Privacy = Enum.Parse<Privacy>(section.Value);
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
                        card.Privacy = Enum.Parse<Privacy>(section.Value);
                        break;
                    case "Orient":
                        card.Orientation = Enum.Parse<Orientation>(section.Value);
                        break;
                    case "Side":
                        card.SideVisible = Enum.Parse<SideVisible>(section.Value);
                        break;
                    case "Events":
                        card.Events.AddRange(DivideEvents(section));
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
                        tile.Orientation = Enum.Parse<Orientation>(section.Value);
                        break;
                    case "Events":
                        tile.Events.AddRange(DivideEvents(section));
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

                            board.Tiles.Add(possibleTile);
                        }

                        break;
                    }
                }
            }

            return board;
        }

        private bool ParseInfoLines(IReadOnlyList<ElementParameter> infoLines)
        {
            return infoLines != null;
        }

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
                else
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
        
        private (int Type, string[] Arguments)? SeparateParenthesisParameter(string parenthesisParameter)
        {
            string[] dividedLine = parenthesisParameter.Split('(', 2);

            if (dividedLine.Length != 2 || !int.TryParse(dividedLine[0], out int id))
                return null;

            return (id, dividedLine[1][..^1].Split(','));
        }

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
                    }
                    
                    var parts = line.Split('|');

                    if (parts.Length != 6)
                        return null;

                    actions = new List<ActionParameter>(int.Parse(parts[5]));

                    var eventDescriptors = SeparateParenthesisParameter(parts[1]);

                    if (!eventDescriptors.HasValue)
                        return null;

                    var valuedEventDescriptors = eventDescriptors.Value;

                    currentEventParameter = new EventParameter
                    {
                        Type = (EventType)valuedEventDescriptors.Type,
                        Priority = int.Parse(parts[3]), 
                        Conditional = DivideArgument(parts[4]),
                        Arguments = valuedEventDescriptors.Arguments.Select(DivideArgument).ToList(),
                    };
                }
                else
                {
                    if (actions.Capacity == actions.Count)
                        return null;
                    
                    actions.Add(DivideAction(line));
                }
            }
            
            if(currentEventParameter != null)
                events.Add(currentEventParameter);

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
                    Arguments = null,
                    Result = result,
                };
            }

            var argument = new ArgumentParameter
            {
                Type = argumentType,
                Arguments = valuedArgumentDescriptors.Arguments.Select(DivideArgument).ToList(),
                Result = null,
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
                
            return new ActionParameter
            {
                Type = (ActionType)valuedActionDescriptors.Type, 
                Conditional = DivideArgument(parts[1]),
                Arguments = valuedActionDescriptors.Arguments.Select(DivideArgument).ToList(),
            };
        }
        
        private static bool StringIsEmptyNullOrWhitespace(string s) =>
            string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s);
    }
}