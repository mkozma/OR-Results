using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_Results
{
    public class Log : ILog
    {
        public string Message { get; set; }

        public void WriteLog(string message)
        {
            Console.WriteLine(message);
        }
    }
}
