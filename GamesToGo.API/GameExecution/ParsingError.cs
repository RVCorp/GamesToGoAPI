namespace GamesToGo.API.GameExecution
{
    public enum ParsingError
    {
        Ok,
        InfoHeader,
        ObjectsHeader,
        Version,
        ParameterGroup,
        Token,
        Card,
        Tile,
        UnknownObject,
        Object,
        InfoLines,
        Board,
        PreparationTurn,
        VictoryConditions,
        Turns,
        Null,
        NoFile,
        WrongHash,
    }
}