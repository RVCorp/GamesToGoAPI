using System.Collections.Generic;
using Newtonsoft.Json;

namespace GamesToGo.API.Models
{
    public partial class UserType
    {
        public UserType()
        {
            User = new HashSet<User>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<User> User { get; set; }
    }
}
