using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;

namespace GamesToGo.API.GameExecution
{
    public class Tile
    {
        public Tile(int typeID)
        {
            TypeID = typeID;
        }

        public int TypeID { get; }

        public IReadOnlyList<Token> Tokens => TokenDictionary.Values.ToList();

        [JsonIgnore]
        public readonly Dictionary<int, Token> TokenDictionary = new Dictionary<int, Token>();
        
        public List<Card> Cards { get; } = new List<Card>();

        public List<EventParameter> Events { get; } = new List<EventParameter>();
        
        public Orientation Orientation { get; set; }

        public Vector2 Arrangement { get; set; }
    }
}