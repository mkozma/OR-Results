﻿using OR_Results.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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

        

        public static string GetCurrentTime()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        public static bool ResultsFileExists()
        {
            string filename = new Settings().FullPath + "results.csv";
            bool torf = File.Exists(filename);
            return torf;
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
            var numberStarted = competitorResultSummaries.Where(c => c.Status == (int)Status.Started).Count();
            var numberDidNotStart = competitorResultSummaries.Where(c => c.Status == (int)Status.DidNotStart).Count();
            var numberFinished = competitorResultSummaries.Where(c => c.Status == (int)Status.Finished).Count();
            var numberMispunch = competitorResultSummaries.Where(c => c.Status == (int)Status.Mispunch).Count();
            var numberDidNotFinish = competitorResultSummaries.Where(c => c.Status == (int)Status.DidNotFinish).Count();
            var total = competitorResultSummaries.Count();

            return total - numberFinished - numberDidNotFinish - numberMispunch;
        }


        public static bool IsFileExists(string filename)
        {
            return File.Exists(filename);
        }

        public static bool isImageFileExists(string filename)
        {
            if (File.Exists(@"C:/Inetpub/wwwroot/Images/"+filename + ".jpg"))
            {
                return true;
            }
            return false;
        }

        public static string GetTimeDiffFromLeader(TimeSpan? leaderTime, TimeSpan? thisTime, int status, int i)
        {
            if 
                ((i == 0) || 
                (status == (int)Status.DidNotStart) || 
                status == (int)Status.Mispunch)
                    return string.Empty;

            var sTimeDiff = string.Empty;

            var timeDiff = thisTime - leaderTime;

            if (timeDiff != null)
            {
                sTimeDiff = ToShortForm((TimeSpan)timeDiff);
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

        public static string GetClubImage(int SI, CompetitorService competitorService)
        {
            string clubImagePath = string.Empty;

            var club = competitorService.Get(SI).Club;

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

        public static string GetElapsedTime(TimeSpan elapsedTime, int status = 0)
        {
            if ((elapsedTime == TimeSpan.MaxValue) ||
                (elapsedTime == TimeSpan.Zero) ||
                (status != (int)Status.Finished) ||
                (elapsedTime < TimeSpan.Zero))
                return string.Empty;
            else
                return elapsedTime.ToString();
        }

        public static string GetVersion()
        {
            return "Version: "+ Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public static string GetCourseHeader(
            List<DisplayTable> listDisplayTable,
            DisplayTable displayTable, 
            DisplayCourse displayCourse)
        {
            if (displayTable.ListDisplayCourses.First() != displayCourse)
                return displayCourse.CourseId;

            if (displayTable.Id == 1)
                return displayCourse.CourseId;

            var previousCourse = listDisplayTable.Single(c => c.Id == displayTable.Id-1).ListDisplayCourses.Last().ListDisplayRows.Last().Course;

            var compare = (displayCourse.CourseId == previousCourse) ? true : false;

            return (compare == true) ? previousCourse + "..." : displayCourse.CourseId;
        }
    }
}
