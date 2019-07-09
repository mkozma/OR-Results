using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_Results
{
    public class HTMLHelper
    {
        public string Course { get; set; }
        public int CourseCount { get; set; }

        public int ClassCountMen { get; set; }
        public int ClassCountWomen { get; set; }

        public bool IsNewTable { get; set; }
    }
}
