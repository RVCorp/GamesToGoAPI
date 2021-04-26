using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GamesToGo.API.GameExecution
{
    public class Board
    {
        public Board(int typeID)
        {
            TypeID = typeID;
        }
        
        public int TypeID { get; }

        public List<Tile> Tiles { get; } = new List<Tile>();

        public Tile this[int x, int y] => Tiles.SingleOrDefault(t => t.Arrangement == new Vector2(x, y));
        
        public bool[] Visibility { get; set; }
    }
}