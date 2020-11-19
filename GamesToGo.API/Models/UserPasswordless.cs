using System.ComponentModel.DataAnnotations.Schema;
using GamesToGo.API.GameExecution;

namespace GamesToGo.API.Models
{
    public class UserPasswordless
    {
        public UserPasswordless(User user)
        {
            this.Room = user.Room;
            this.Id = user.Id;
            this.Username = user.Username;
            this.Email = user.Email;
            this.UsertypeId = user.UsertypeId;
            this.Image = user.Image;
        }

        [NotMapped]
        public Room Room { get; set; }
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int UsertypeId { get; set; }
        public string Image { get; set; }
    }
}
