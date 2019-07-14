using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_Results
{
    public static class Shared
    {
        public static string GetEnumValue(int value)
        {
            Status enumDisplayStatus = (Status)value;
            return enumDisplayStatus.ToString();
        }

        public static string GetGenderFromClass(string className)
        {
            var gender = string.Empty;
            return gender = className.Contains("Women") ? "women" : "men";
        }

        public static Course GetCourseDetails(string courseId)
        {
            return Program.courses.FirstOrDefault(c => c.CourseId == courseId);
        }

        public static string GetCurrentTime()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        public static string GetCompetitorClass(int SI)
        {
            return Program.competitors.FirstOrDefault(c => c.SI == SI).ClassId;
        }

        public static bool ResultsFileExists()
        {
            string filename = new Settings().FullPath + "results.csv";
            bool torf = File.Exists(filename);
            return torf;
        }

        public static string GetName(int SI)
        {
            return Program.competitors.FirstOrDefault(c => c.SI == SI).Name;
        }
    }
}
