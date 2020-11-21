using GamesToGo.API.Models;
using Newtonsoft.Json;

namespace GamesToGo.API.GameExecution
{
    public class Player
    {
        public int RoomPosition { get; set; }
        
        [JsonProperty(@"User")]
        public UserPasswordless BackingUser { get; }
        
        public bool Ready { get; set; }

        public Tile Tile { get; } = new Tile();

        public Player(UserPasswordless user)
        {
            BackingUser = user;
        }
    }
}