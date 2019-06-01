using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_Results
{
    public class CourseVariant : CSVHelper<CourseVariant>
    {
        public int Id { get; set; }
        public string CourseId { get; set; }
        public string Field3 { get; set; }
        public string Field4 { get; set; }
        public string Field5 { get; set; }
        public string Field6 { get; set; }
        public string Field7 { get; set; }
        public List<int> CourseControls { get; set; }

    }
}
