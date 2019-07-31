using System.Collections.Generic;

namespace OR_Results
{
    public class CourseVariant: CSVHelper<CourseVariant>
    {
        public int CourseVariantId { get; set; }
        public string CourseId { get; set; }
        public string Field1 { get; set; }
        public double Field2 { get; set; }
        public string Field3 { get; set; }
        public int Field4 { get; set; }
        public int Field5 { get; set; }
        public List<string> Controls { get; set; }
    }
}