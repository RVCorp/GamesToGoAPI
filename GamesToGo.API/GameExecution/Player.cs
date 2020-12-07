using GamesToGo.API.Models;
using Newtonsoft.Json;

namespace GamesToGo.API.GameExecution
{
    public class Player
    {
        public int RoomPosition { get; set; }
        
        [JsonProperty(@"User")]
        public User BackingUser { get; }
        
        public bool Ready { get; set; }

        public Tile Tile { get; } = new Tile(-1);

        public Player(User user)
        {
            BackingUser = user;
        }
    }
}