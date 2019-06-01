using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_Results
{
    public class Control: CSVHelper<Control>
    {
        public string Name { get; set; }
        public int Field1 { get; set; }
        public int Field2 { get; set; }
        public int Score { get; set; }
        public string Field3 { get; set; }
        public bool Field4 { get; set; }
    }
}
