using System;
using System.Reflection;

namespace GamesToGo.API.GameExecution
{
    public enum ArgumentType
    {
        DefaultArgument,
        CardWithToken,
        [ShouldHaveResultAttibute]
        CardType,
        CompareCardTypes,
        ComparePlayerHasCardType,
        ComparePlayerHasNoCardType,
        ComparePlayerHasTokenType,
        ComparePlayerWithTokenHasMoreThanXTokens,
        ComparePlayerWithTokenHasXTokens,
        CompareXPositionInTileIsNotCardTypeArgument,
        FirstXCardsFromTile,
        [ShouldHaveResultAttibute]
        NumberArgument,
        PlayerCardsWithToken,
        PlayerChosenByPlayer,
        PlayerRightOfPlayerWithToken,
        PlayerWithToken,
        [ShouldHaveResultAttibute]
        PrivacyType,
        [ShouldHaveResultAttibute]
        TileType,
        [ShouldHaveResultAttibute]
        TokenType,
    }

    public class ShouldHaveResultAttibute : Attribute
    {
        
    }

    public static class ArgumentTypeExtensions
    {
        public static bool ShouldHaveResult(this ArgumentType type)
        {
            return type.GetType().GetField(type.ToString())?.GetCustomAttribute<ShouldHaveResultAttibute>() != null;
        }
    }
}