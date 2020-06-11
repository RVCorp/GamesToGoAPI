using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GamesToGoAPI.Models.GameSettings
{
    public class Room
    {
        public int id { get; set; }
        private List<User> users = new List<User>();
        private GamesToGoContext _context;
        public Room(int id, String userID, GamesToGoContext context)
        {
            this.id = id;
            _context = context;
            users.Add(_context.User.ToList().Where(x => x.Id == Int32.Parse(userID)).FirstOrDefault());
        }

        public void JoinRoom(int id, string userID)
        {
            users.Add(_context.User.ToList().Where(x => x.Id == Int32.Parse(userID)).FirstOrDefault());
        }
    }
}
