using System.Collections.Generic;

namespace GamesToGo.API.GameExecution
{
    public class Tile
    {
        public int ID { get; set; }
        
        public int TypeID { get; set; }
        
        public readonly List<Token> Tokens = new List<Token>();
        
        public readonly List<Card> Cards = new List<Card>();
    }
}