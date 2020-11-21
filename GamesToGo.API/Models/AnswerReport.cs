namespace GamesToGo.API.Models
{
    public partial class AnswerReport
    {
        public int Id { get; set; }
        public string Details { get; set; }
        public int AnswertypeId { get; set; }
        public int AdminId { get; set; }
        public int ReportId { get; set; }

        public virtual User Admin { get; set; }
        public virtual AnswerType Answertype { get; set; }
        public virtual Report Report { get; set; }
    }
}
