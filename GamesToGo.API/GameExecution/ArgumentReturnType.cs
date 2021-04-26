using System;
using System.Reflection;

namespace GamesToGo.API.GameExecution
{
    public enum ArgumentReturnType
    {
        Default = 0,

        Single = 1 << 0,
        Multiple = 1 << 1,
        Type = 1 << 2,

        Player = 1 << 3,
        Number = 1 << 4,
        Card = 1 << 5,
        Tile = 1 << 6,
        Token = 1 << 7,
        Board = 1 << 8,
        Comparison = 1 << 9,

        Privacy = 1 << 10,
        Orientation = 1 << 11,
        Direction = 1 << 12,

        SinglePlayer = Single | Player,
        MultiplePlayer = Multiple | Player,

        SingleNumber = Single | Number,
        MultipleNumber = Multiple | Number,

        SingleCard = Single | Card,
        MultipleCard = Multiple | Card,
        CardType = Card | Type,

        SingleTile = Single | Tile,
        MultipleTile = Multiple | Tile,
        TileType = Tile | Type,

        SingleToken = Single | Token,
        MultipleToken = Multiple | Token,
        TokenType = Token | Type,

        SingleBoard = Single | Board,
        MultipleBoard = Multiple | Board,
        BoardType = Board | Type,
    }

    public static class ArgumentReturnTypeExtensions
    {
        public static ArgumentReturnType[] InnerReturnTypes(this ArgumentType type)
        {
            return type.GetType().GetField(type.ToString())?.GetCustomAttribute<InnerReturnTypesAttribute>()
                ?.InnerReturnTypes;
        }

        public static ArgumentReturnType[] InnerReturnTypes(this EventType type)
        {
            return type.GetType().GetField(type.ToString())?.GetCustomAttribute<InnerReturnTypesAttribute>()
                ?.InnerReturnTypes;
        }

        public static ArgumentReturnType ReturnType(this ArgumentType type)
        {
            return type.GetType().GetField(type.ToString())?.GetCustomAttribute<ReturnTypeAttribute>()
                ?.ReturnType ?? ArgumentReturnType.Default;
        }

        public static ArgumentReturnType[] InnerReturnTypes(this ActionType type)
        {
            return type.GetType().GetField(type.ToString())?.GetCustomAttribute<InnerReturnTypesAttribute>()
                ?.InnerReturnTypes;
        }
    }

    public class ReturnTypeAttribute : Attribute
    {
        public ArgumentReturnType ReturnType { get; }
        
        public ReturnTypeAttribute(ArgumentReturnType returnType)
        {
            ReturnType = returnType;
        }
    }
}