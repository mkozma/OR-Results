using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_Results
{
    public class Competitor : CSVHelper<Competitor>
    {
        public int Id { get; set; }
        public int SI { get; set; }
        public string Name { get; set; }
        public string Club { get; set; }
        public string CourseId { get; set; }
        public bool Field1 { get; set; }
        public string ClassId { get; set; }
        public int Field2 { get; set; }
        public int Field3 { get; set; }
        public int Field4 { get; set; }
        public bool Field5 { get; set; }
        public string Field6 { get; set; }
        public int Field7 { get; set; }
        public int Field8 { get; set; }
        public int Field9 { get; set; }
        public int Field10 { get; set; }
        public string Field11 { get; set; }
        public int Field12 { get; set; }
        public bool Field13 { get; set; }
    }
}
