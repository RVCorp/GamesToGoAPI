using System.ComponentModel;
using Newtonsoft.Json;

namespace GamesToGo.API.Models
{
    public class UserStatistic
    {
        [JsonIgnore]
        public int ID { get; set; }
        public UserStatisticType Type { get; set; }
        public int Amount { get; set; }
        
        [JsonIgnore]
        public virtual User User { get; set; }
    }

    public class NamedStatistic
    {
        public string Name { get; set; }
        public int Amount { get; set; }
    }

    public enum UserStatisticType
    {
        [Description(@"Juegos jugados")]
        GamesPlayed,
        [Description(@"Victorias")]
        Victories,
    }
}