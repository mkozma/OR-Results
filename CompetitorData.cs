using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_Results
{
    public class CompetitorData
    {
        private string _filename;
        private string _delimiter;
        private bool _hasHeaderRecord;

        public List<Competitor> Competitors;
        public CompetitorData(string filename, string delimiter, bool hasHeaderRecord)
        {
            _filename = filename;
            _delimiter = delimiter;
            _hasHeaderRecord = hasHeaderRecord;
        }

        public void ReadFileData()
        {
            using (var reader = new StreamReader(_filename))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.Delimiter = _delimiter;
                csv.Configuration.HasHeaderRecord = _hasHeaderRecord;
                Competitors = csv.GetRecords<Competitor>().ToList();
            }
        }
    }
}
