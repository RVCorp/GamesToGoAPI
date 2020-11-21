using System.Collections.Generic;

namespace GamesToGo.API.GameExecution
{
    public class Board
    {
        public int TypeID { get; }
        public int ID { get; }
        public List<Tile> Tiles;
    }
}