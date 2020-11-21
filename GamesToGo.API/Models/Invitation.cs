using GamesToGo.API.GameExecution;
using Newtonsoft.Json;

namespace GamesToGo.API.Models
{
    public class Invitation
    {
        public UserPasswordless Transmitter { get; }
        public UserPasswordless Receiver { get; }
        
        [JsonIgnore]
        public Room Room { get; }

        public Invitation(UserPasswordless transmitterID, UserPasswordless receiverID, Room room)
        {
            Transmitter = transmitterID;
            Receiver = receiverID;
            Room = room;
        }

        [JsonProperty(@"Room")]
        public RoomPreview RoomPreview => (RoomPreview) Room;
    }
}
