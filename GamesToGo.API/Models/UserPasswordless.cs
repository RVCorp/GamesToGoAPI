using System.ComponentModel.DataAnnotations.Schema;
using GamesToGo.API.GameExecution;
using Newtonsoft.Json;

namespace GamesToGo.API.Models
{
    public class UserPasswordless
    {
        [JsonIgnore] 
        public readonly User User;
        public UserPasswordless(User user)
        {
            User = user;
        }

        [JsonIgnore]
        [NotMapped]
        public Room Room
        {
            get => User.Room;
            set => User.Room = value;
        }

        public int Id => User.Id;
        public string Username => User.Username;
        public string Email => User.Email;
        public int UsertypeId => User.UsertypeId;

        public string Image
        {
            get => User.Image;
            set => User.Image = value;
        }
    }
}
