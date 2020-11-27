using System;
using GamesToGo.API.GameExecution;
using Newtonsoft.Json;

namespace GamesToGo.API.Models
{
    public class Invitation
    {
        public int ID { get; set; }
        
        public DateTime TimeSent { get; set; }
        public User Sender { get; set; }
        public User Receiver { get; set; }
        
        [JsonIgnore]
        public Room Room { get; set; }

        [JsonProperty(@"Room")]
        public RoomPreview RoomPreview => (RoomPreview) Room;
    }
}
