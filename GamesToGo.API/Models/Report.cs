using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GamesToGo.API.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string Reason { get; set; }
        
        public DateTime TimeReported { get; set; }
        public virtual Game Game { get; set; }
        public virtual ReportType ReportType { get; set; }
        public virtual User User { get; set; }
    }
}
