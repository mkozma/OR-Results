using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_Results
{
    public class DisplayCourse
    {
        public int Id { get; set; }
        public string CourseId { get; set; }
        public TimeSpan? LeaderTime { get; set; }
        public List<DisplayRow> ListDisplayRows { get; set; }
    }
}
