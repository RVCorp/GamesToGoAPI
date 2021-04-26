using System.Collections.Generic;

namespace GamesToGo.API.GameExecution
{
    public class Card
    {
        public Card(int typeID, int id = 0)
        {
            TypeID = typeID;
            ID = id;
        }

        public int ID { get; }

        public int TypeID { get; }
        
        public Orientation Orientation { get; set; }
        
        public Privacy Privacy { get; set; }

        public SideVisible SideVisible { get; set; }

        public List<EventParameter> Events { get; } = new List<EventParameter>();

        public List<Token> Tokens { get; } = new List<Token>();

        public void MoveFrom(Tile tile)
        {
            
        }

        public void MoveTo(Tile tile)
        {
            
        }

        public Card CloneEmpty(int id)
        {
            var card = new Card(TypeID, id)
            {
                Orientation = Orientation, 
                Privacy = Privacy, 
                SideVisible = SideVisible,
            };
            return card;
        }
    }
}