using System.Collections.Generic;

namespace GamesToGo.API.GameExecution
{
    public class Board
    {
        public Board(int typeID)
        {
            TypeID = typeID;
        }

        public int TypeID { get; }
        public List<Tile> Tiles { get; set; }
        public bool[] Visibility { get; set; }
    }
}