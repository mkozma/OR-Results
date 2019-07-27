using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.Collections.Generic;

namespace OR_Results
{
    public class CSVHelper<T>
    {
        //private const string FILENAME_PREFIX = @"C:\Users\mkozm\OR\21\";
        //private string FILENAME_SUFFIX = @".csv";
        
        public IEnumerable<T> ReadData(string filename, T t, string delimiter=",", bool hasHeaderRecord = false)
        {
            using (var reader = new StreamReader(filename))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.Delimiter = delimiter;
                csv.Configuration.HasHeaderRecord = hasHeaderRecord;
                csv.Configuration.MissingFieldFound = null;
                return csv.GetRecords<T>().ToList();
            }
        }
    }
}
