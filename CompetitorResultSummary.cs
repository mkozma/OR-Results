//using HtmlAgilityPack;
using System;

namespace OR_Results
{
    public class CompetitorResultSummary
    {
        public int SI { get; set; }
        public string CourseId { get; set; }
        public int Status { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public TimeSpan? ElapsedTime { get; set; }
        public int Score { get; set; }
    }
}
