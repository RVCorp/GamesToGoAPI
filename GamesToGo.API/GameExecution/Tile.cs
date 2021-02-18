using System.Collections.Generic;

namespace GamesToGo.API.GameExecution
{
    public class Tile
    {
        public Tile(int typeID)
        {
            TypeID = typeID;
        }

        public int TypeID { get; }
        
        public List<Token> Tokens { get; } = new List<Token>();
        
        public List<Card> Cards { get; } = new List<Card>();

        public List<EventParameter> Events { get; } = new List<EventParameter>();
        
        public Orientation Orientation { get; set; }
    }
}