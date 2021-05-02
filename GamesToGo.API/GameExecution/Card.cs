using System.Collections.Generic;
using System.Linq;

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

        public List<Token> Tokens => TokenDictionary.Values.ToList();

        public readonly Dictionary<int, Token> TokenDictionary = new Dictionary<int, Token>();
        
        public Card CloneEmpty(int id)
        {
            var card = new Card(TypeID, id)
            {
                Orientation = Orientation, 
                Privacy = Privacy, 
                SideVisible = SideVisible,
            };
            
            card.Events.AddRange(Events.Select(e => e.Clone()));
            return card;
        }
    }
}