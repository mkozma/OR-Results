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

        public static bool CompetitorCourseExists(string competitorCourseId)
        {
            var x = Program.courses.Any(c => c.CourseId == competitorCourseId);
            return x;

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

        public static bool IsFileExists(string filename)
        {
            return File.Exists(filename);
        }

        public static string GetClub(int sI)
        {
            var club = Program.competitors.SingleOrDefault(c => c.SI == sI).Club;
            var hasSpace = club.Contains(" ");
            return (hasSpace) ? club.Replace(" ", "%20") : club;
        }

        public static bool isImageFileExists(string filename)
        {
            if (File.Exists(@"C:/Inetpub/wwwroot/Images/"+filename + ".jpg"))
            {
                return true;
            }
            return false;
        }

        public static string GetTimeDiffFromLeader(TimeSpan? leaderTime, TimeSpan? thisTime)
        {
            var sTimeDiff = string.Empty;

            var x = thisTime - leaderTime;

            if (x != null)
            {
                sTimeDiff = ToShortForm((TimeSpan)x);
            }
            return sTimeDiff;
        }

        public static string ToShortForm(TimeSpan t)
        {
            string shortForm = "";
            if (t.Hours > 0)
            {
                shortForm += string.Format("{0}h", t.Hours.ToString());
            }
            if (t.Minutes > 0)
            {
                shortForm += string.Format("{0}m", t.Minutes.ToString());
            }
            if (t.Seconds > 0)
            {
                shortForm += string.Format("{0}s", t.Seconds.ToString());
            }
            return shortForm;
        }

        public static string GetClubImage(int SI)
        {
            string clubImagePath = string.Empty;

            var club = Program.competitors.SingleOrDefault(c => c.SI == SI).Club;


            if (!isImageFileExists(club + Constants.IMAGE_SIZE ))
            {

                return clubImagePath = String.Format("{0}",  "OV" + Constants.IMAGE_SIZE + Constants.IMAGE_FILE_EXTENSION);
            }

            if (club == "%20")
                return "spacer";

            var hasSpace = club.Contains(" ");
            club = (hasSpace) ? club.Replace(" ", "%20") : club;
            return clubImagePath = String.Format("{0}",  club + Constants.IMAGE_SIZE + Constants.IMAGE_FILE_EXTENSION);
        }
        public static string GetElapsedTime(int SI)
        {
            //var elapsedTime = course.Items[i - 1].ElapsedTime;
            var elapsedTime = Program.CompetitorCourseSummaries.FirstOrDefault(c => c.SI == SI).ElapsedTime;
            var status = Program.CompetitorCourseSummaries.FirstOrDefault(c => c.SI == SI).Status;

            if ((elapsedTime == TimeSpan.MaxValue) ||
                (elapsedTime == TimeSpan.Zero) ||
                (status != (int)Status.Finished) ||
                (elapsedTime < TimeSpan.Zero)) 
                return string.Empty;
            else
                return elapsedTime.ToString();


        }
    }
}
