﻿using System.Collections.Generic;

namespace GamesToGo.API.Models
{
    public partial class AnswerType
    {
        public AnswerType()
        {
            AnswerReport = new HashSet<AnswerReport>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<AnswerReport> AnswerReport { get; set; }
    }
}