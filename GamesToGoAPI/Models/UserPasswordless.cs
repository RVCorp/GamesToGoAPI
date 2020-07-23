using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GamesToGoAPI.Models
{
    public class UserPasswordless
    {
        public UserPasswordless(User user)
        {
            this.RoomID = user.RoomID;
            this.Id = user.Id;
            this.Username = user.Username;
            this.Email = user.Email;
            this.UsertypeId = user.UsertypeId;
            this.Image = user.Image;
        }

        [NotMapped]
        public int RoomID { get; set; }
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int UsertypeId { get; set; }
        public string Image { get; set; }
    }
}
