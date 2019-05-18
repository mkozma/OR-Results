//using HtmlAgilityPack;
using System.Collections.Generic;

namespace OR_Results
{
    public class CompetitorResult
    {
        public int SI { get; set; }
        public string Punch1 { get; set; }
        public string Punch2 { get; set; }
        public string StartPunchTime { get; set; }
        public string FinishPunchTime { get; set; }
        public List<string> ControlPunches { get; set; }

    }
}
