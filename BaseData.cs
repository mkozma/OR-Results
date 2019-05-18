using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_Results
{
    public class BaseData
    {
        private readonly string _filename;
        private readonly string _delimiter;
        private readonly bool _hasHeaderRecord;
        public List<dynamic> Records { get; set; }
        public BaseData(string filename, string delimiter, bool hasHeaderRecord)
        {
            _filename = filename;
        }
        public void ReadFileData()
        {
            using (var reader = new StreamReader(_filename))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.Delimiter = _delimiter;
                csv.Configuration.HasHeaderRecord = _hasHeaderRecord;
                Records = csv.GetRecords<dynamic>().ToList();
            }
        }
    }
}
