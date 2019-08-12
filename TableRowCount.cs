using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_Results
{
    public class TableRowCount
    {
        public int Id { get; set; }
        public int DataToSkip { get; set; }
        public int DataToTake { get; set; }
        public string LastCourse { get; set; }
        public int LastCourseCount { get; set; }
        public int LastMensClassCount { get; set; }
        public int LastWomensClassCount { get; set; }
        public bool ContinuedCourse { get; set; }
    }
}
