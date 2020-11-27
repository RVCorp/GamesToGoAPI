namespace GamesToGo.API.Models
{
    public class UserLogin
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        
        public virtual User User { get; set; }
    }
}