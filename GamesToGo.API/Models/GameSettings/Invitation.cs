using GamesToGo.API.Controllers;
using GamesToGo.API.Models.GameSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamesToGo.API.GameExecution;

namespace GamesToGo.API.Models.GameSettings
{
    public class Invitation
    {
        public int Transmitter { get; set; }
        public int Receiver { get; set; }
        public Room Room { get; set; }


        public Invitation(int transmitterID, int receiverID, Room room)
        {
            Transmitter = transmitterID;
            Receiver = receiverID;
            Room = room;
        }
    }
}
