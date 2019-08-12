using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_Results
{
    public class DisplayRow
    {

        public int Id { get; set; }

        public string Course { get; set; }
        public string Class { get; set; }
        public string ClassCount { get; set; }
        public int Status { get; set; }
        public string SI { get; set; }
        public string Name { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public string TimeDifference { get; set; }
        public int Penalty { get; set; }
        public int NetScore { get; set; }

        public DisplayRow()
        {
        }

    }
}
