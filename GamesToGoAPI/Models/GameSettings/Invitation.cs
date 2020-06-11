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
        public int transmitter { get; set; }
        public int receiver { get; set; }
        public Room room { get; set; }

        private RoomController rc;

        public Invitation(int transmitterID, int receiverID, Room room, GamesToGoContext context)
        {
            transmitter = transmitterID;
            receiver = receiverID;
            this.room = room;
        }
    }
}
