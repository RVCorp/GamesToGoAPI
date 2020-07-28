using System.Collections.Generic;

namespace GamesToGoAPI.Models
{
    public partial class Report
    {
        public Report()
        {
            AnswerReport = new HashSet<AnswerReport>();
        }

        public int Id { get; set; }
        public string Reason { get; set; }
        public int UserId { get; set; }
        public int GameId { get; set; }

        public virtual Game Game { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<AnswerReport> AnswerReport { get; set; }
    }
}
