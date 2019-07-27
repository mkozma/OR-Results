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

        public static string GetCourseIdBySI(int SI)
        {
            return Program.competitors.FirstOrDefault(c => c.SI == SI).CourseId;
        }

        public static Course GetCourseBySI(int SI)
        {
            var courseId = Program.competitors.FirstOrDefault(c => c.SI == SI).CourseId;
            if(courseId == null) { return null;  }

            return Program.courses.FirstOrDefault(c => c.CourseId == courseId);
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

        public static TimeSpan GetTimeFromMilliseconds(long input)
        {
            int h = 0;
            int m = 0;
            int s = 0;

            int seconds = (int)input / 1000;

            h = seconds / 3600;
            var remainder = seconds % 3600;

            if (remainder != 0)
            {
                m = remainder / 60;
                s = remainder % 60;
            }
            return new TimeSpan(h, m, s);

        } 

        public static int NumberOfCompetitorsRemaining(List<CompetitorResultSummary> competitorResultSummaries)
        {
            var numberStarted = competitorResultSummaries.Select(c => c.Status == (int)Status.Started).Count();
            var total = competitorResultSummaries.Count();

            return total - numberStarted;
        }

        public static string GetCourseTypeByCourse(string courseId)
        {
            return Program.courses.SingleOrDefault(c => c.CourseId == courseId).CourseType;
        }
    }
}
