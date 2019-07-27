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
        public int XCoordinate { get; set; }
        public int YCoordinate { get; set; }
        public int Score { get; set; }
        public string Display { get; set; }
        public string Ignore { get; set; }
    }
}
