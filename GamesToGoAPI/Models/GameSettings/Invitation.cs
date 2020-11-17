using GamesToGoAPI.Controllers;
using GamesToGoAPI.Models.GameSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GamesToGoAPI.Models.GameSettings
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
