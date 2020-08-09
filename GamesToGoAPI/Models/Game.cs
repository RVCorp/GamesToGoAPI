using System;
using System.Collections.Generic;

namespace GamesToGoAPI.Models
{
    public partial class Game
    {
        public Game()
        {
            Report = new HashSet<Report>();
        }

        public int Id { get; set; }
        public string Image { get; set; }
        public string LastEdited { get; set; }
        public string Name { get; set; }
        public string Hash { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public int Minplayers { get; set; }
        public int Maxplayers { get; set; }
        public int CreatorId { get; set; }

        public virtual User Creator { get; set; }
        public virtual ICollection<Report> Report { get; set; }
    }
}
