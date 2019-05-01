//using HtmlAgilityPack;
using System;

namespace OR_Results
{
    public class CompetitorResultSummary
    {
        public int SI { get; set; }
        public int Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public TimeSpan? ElapsedTime { get; set; }
        public int Score { get; set; }
    }
}
