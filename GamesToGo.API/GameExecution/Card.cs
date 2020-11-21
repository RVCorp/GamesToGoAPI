using System.Collections.Generic;

namespace GamesToGo.API.GameExecution
{
    public class Card
    {
        public int ID { get; }

        public int TypeID { get; }
        
        public int Orientation { get; set; }
        
        public bool FrontVisible { get; set; }

        public List<Token> Tokens { get; } = new List<Token>();

        public void MoveFrom(Tile tile)
        {
            
        }

        public void MoveTo()
        {
            
        }
    }
}