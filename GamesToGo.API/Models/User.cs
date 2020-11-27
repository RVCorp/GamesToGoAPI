using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GamesToGo.API.GameExecution;
using Newtonsoft.Json;

namespace GamesToGo.API.Models
{
    public class User
    {
        public User()
        {
            Game = new HashSet<Game>();
            Report = new HashSet<Report>();
        }

        [NotMapped]
        [JsonIgnore]
        public Room Room { get; set; }
        public int Id { get; set; }
        public string Username { get; set; }
        public string Image { get; set; }
        [NotMapped]
        public DateTime LogoutTime { get; set; }
        [JsonIgnore]
        public virtual ICollection<Game> Game { get; set; }
        [JsonIgnore]
        public virtual ICollection<Report> Report { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<UserStatistic> UserStatistic { get; set; }
    }
}
