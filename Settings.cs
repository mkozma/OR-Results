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

        private string webServerFileLocation;
        private bool openLocalBrowser;
        private bool refreshBrowser;
        private bool timer;
        private int timerInterval;
        private string fullpath;
        private const string backSlash = "\\";

        private const string webFileName = "index.html";

        public  string FullPath 
        {
            get { return dataPath + competition + backSlash; }
        }

        public string DataPath
        {
            get { return dataPath; }
        }

        public string WebServerFileLocation
        {
            get { return webServerFileLocation;  }
            set { webServerFileLocation = value;  }
        }

        public string WebServerFilePath
        {
            get { return webServerFileLocation + webFileName; }
        }

        public bool OpenLocalBrowser
        {
            get { return openLocalBrowser; }
            set { openLocalBrowser = value; }
        }

        public bool RefreshBrowser
        {
            get { return refreshBrowser;  }
            set { refreshBrowser = value;  }
        }

        public bool Timer
        {
            get { return timer; }
            set { timer = value;  }
        }

        public int TimerInterval
        {
            get { return timerInterval; }
            set { timerInterval = value;  }
        }

        public TimeSpan ZeroTime { get; set; }

        public Settings()
        {
            ReadAppConfigSettings();
        }

        public void ReadAppConfigSettings()
        {
            competition = ConfigurationManager.AppSettings["competition"];
            dataPath = ConfigurationManager.AppSettings["datapath"];
            webServerFileLocation = ConfigurationManager.AppSettings["webServerFileLocation"];
            openLocalBrowser = (ConfigurationManager.AppSettings["openLocalBrowser"]=="true"? true: false );
            refreshBrowser = (ConfigurationManager.AppSettings["refreshBrowser"]=="true"? true: false );
            timer = (ConfigurationManager.AppSettings["timer"] == "true" ? true : false);
            timerInterval = Convert.ToInt32(ConfigurationManager.AppSettings["timerInterval"]);

        }
    }
}
