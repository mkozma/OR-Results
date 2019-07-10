using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace OR_Results
{
    public class Settings
    {
        private string competition { get; set; }
        private string dataPath { get; set; }
        private string fullpath;
        private string backSlash = "\\";

        public  string FullPath 
        {
            get { return dataPath + backSlash + competition + backSlash; }
        }


        public Settings()
        {
            ReadAppConfigSettings();
        }

        public void ReadAppConfigSettings()
        {
            competition = ConfigurationManager.AppSettings["competition"];
            dataPath = ConfigurationManager.AppSettings["datapath"];
        }
    }
}
