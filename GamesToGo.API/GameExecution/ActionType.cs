namespace GamesToGo.API.GameExecution
{
    public enum ActionType
    {
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.CardType,
            ArgumentReturnType.TileType,
        })]
        AddCardToToTile = 1,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.SingleCard,
            ArgumentReturnType.Privacy,
        })]
        ChangeCardPrivacy = 2,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.SingleToken,
            ArgumentReturnType.Privacy,
        })]
        ChangeTokenPrivacy = 3,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.SingleNumber,
        })]
        DelayGame = 4,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.TokenType,
            ArgumentReturnType.SinglePlayer,
            ArgumentReturnType.SinglePlayer,
        })]
        GivePlayerATokenTypeFromPlayer = 5,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.TokenType,
            ArgumentReturnType.SinglePlayer,
        })]
        RemoveTokenTypeFromPlayer = 6,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.SinglePlayer,
        })]
        RemovePlayer = 7,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.CardType,
            ArgumentReturnType.SinglePlayer,
            ArgumentReturnType.SingleTile,
        })]
        MoveCardFromPlayerToTile = 8,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.CardType,
            ArgumentReturnType.SinglePlayer,
            ArgumentReturnType.SingleTile,
            ArgumentReturnType.SingleNumber,
        })]
        MoveCardFromPlayerToTileInXPosition = 9,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.SingleTile,
            ArgumentReturnType.SingleNumber,
        })]
        DelayTile = 10,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.SingleTile,
        })]
        ShuffleTile = 11,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.SingleNumber,
            ArgumentReturnType.SingleTile,
            ArgumentReturnType.SinglePlayer,
        })]
        GivePlayerXCardsFromTileAction = 12,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.SingleNumber,
            ArgumentReturnType.SinglePlayer,
            ArgumentReturnType.SingleTile,
        })]
        MoveXCardsFromPlayerToTile = 13,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.TokenType,
            ArgumentReturnType.SingleNumber,
            ArgumentReturnType.SingleTile,
        })]
        GiveXCardsAToken = 14,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.TokenType,
            ArgumentReturnType.MultipleCard,
        })]
        RemoveTokenTypeFromCard = 15,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.TokenType,
            ArgumentReturnType.SinglePlayer,
        })]
        GivePlayerATokenType = 16,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.SingleCard,
            ArgumentReturnType.SinglePlayer,
            ArgumentReturnType.SinglePlayer,
        })]
        GiveCardFromPlayerToPlayer = 17,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.SingleNumber,
            ArgumentReturnType.TokenType,
            ArgumentReturnType.SinglePlayer,
        })]
        GivePlayerXTokensTypeAction = 18,
        
        [InnerReturnTypes(new ArgumentReturnType[0])]
        StopTileEvents = 19,
        
        [InnerReturnTypes(new ArgumentReturnType[0])]
        StopTileDelay = 20,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.SingleCard,
            ArgumentReturnType.SingleTile,
            ArgumentReturnType.SingleTile,
        })]
        MoveCardFromPlayerTileToTile = 22,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.SinglePlayer,
            ArgumentReturnType.Comparison,
        })]
        PlayerWins = 23,
        
        [InnerReturnTypes(new []
        {
            ArgumentReturnType.SinglePlayer,
            ArgumentReturnType.Comparison,
        })]
        PlayerStatesOfDefeat = 24,
    }
}