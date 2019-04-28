using CsvHelper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OR_Results
{
    class Program
    {
        private static List<CompetitorResult> records;
        private static List<CoursePunch> coursePunches;
        private static List<CompetitorResultSummary> competitorCourseSummaries;
        private static List<Control> controls;
        private static string Course;

        static void Main(string[] args)
        {
            GetCourseData();
            GetResultsData();

            Results();
        }

        private static void GetCourseData()
        {
            using (var reader = new StreamReader(@"C:\Users\mkozm\Or\2\Controls.csv"))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.Delimiter = ",";
                csv.Configuration.HasHeaderRecord = false;
                controls = csv.GetRecords<Control>().ToList();
            }
        }

        private static void Results()
        {
            GenerateResults();
            DisplayResults();
        }

        private static void GenerateResults()
        {
            var competitorCourseSummaries = new List<CompetitorResultSummary>();

            var competitorCourseSummary = new CompetitorResultSummary();
            competitorCourseSummary.SI = coursePunches.Take(1).DefaultIfEmpty().FirstOrDefault().SI;
            competitorCourseSummary.StartTime = ParseCourseAndTime(coursePunches.Where(s => s.CoursePunchName == "S").FirstOrDefault().CoursePunchTime);
            competitorCourseSummary.Status = GetCompetitorStatus(coursePunches);
            if(competitorCourseSummary.Status == (int)Status.Finished)
            {
                CalculateElapsedTime(competitorCourseSummary);
                CalculateScore(competitorCourseSummary);

            }

        }

        private static void CalculateScore(CompetitorResultSummary competitorCourseSummary)
        {
            var competitorCourse = "A";
            var isScoreCourse = true;
            if (isScoreCourse)
                CalculateScoreCoursePoints();
        }

        private static void CalculateScoreCoursePoints()
        {
            var totPoints = 0;
            foreach(var coursePunch in coursePunches)
            {
                totPoints += GetCoursePunchScore(coursePunch.CoursePunchName);
            }
        }

        private static int GetCoursePunchScore(string coursePunchName)
        {
            if ((coursePunchName=="S")|| (coursePunchName=="F"))
                return 0;
            return controls.Where(s => s.Name == coursePunchName).FirstOrDefault().Score;

        }

        private static void CalculateElapsedTime(CompetitorResultSummary competitorCourseSummary)
        {
            competitorCourseSummary.FinishTime = ParseCourseAndTime(coursePunches.Where(s => s.CoursePunchName == "F").FirstOrDefault().CoursePunchTime);
            competitorCourseSummary.ElapsedTime = (competitorCourseSummary.FinishTime == null)? null:
                competitorCourseSummary.ElapsedTime = competitorCourseSummary.FinishTime - competitorCourseSummary.StartTime;
        }

        private static DateTime ParseCourseAndTime(string courseAndTime)
        {
            int index = courseAndTime.IndexOf(':');
            Course = courseAndTime.Substring(0, index);
            var theRest = courseAndTime.Substring(index + 1, courseAndTime.Length - Course.Length-1);
            index = theRest.IndexOf(':');
            var theDay = theRest.Substring(0, index);
            var theTime = theRest.Substring(index + 1, theRest.Length - theDay.Length-1);

            return DateTime.Parse(theTime);
        }

        private static int GetCompetitorStatus(List<CoursePunch> coursePunches)
        {
            if (coursePunches.Where(s => s.CoursePunchName == "F").FirstOrDefault() != null)
                return (int)Status.Finished;
            else if ((coursePunches.Where(s => s.CoursePunchName == "F").FirstOrDefault() == null)
                    && (coursePunches.Where(s => s.CoursePunchName == "S").FirstOrDefault() != null))
                return (int)Status.Started;
            else  return (int)Status.DidNotStart;
        }

        private static void DisplayResults()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(@" < html >< body >< div id = 'foo' > text </ div ></ body ></ html > ");
            var div = doc.GetElementbyId("foo");

            // Show info
            System.Console.WriteLine(div.OuterHtml);

            // Show info
            //FiddleHelper.WriteTable(new List<string>() { div.OuterHtml });

            // Show info
            //FiddleHelper.WriteTable(new List<HtmlAgilityPack.HtmlNode>() { div });
        }

        private static void GetResultsData()
        {
            using (var reader = new StreamReader(@"C:\Users\mkozm\Or\2\Results.csv"))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.Delimiter = ";";
                csv.Configuration.HasHeaderRecord = false;
                records = csv.GetRecords<CompetitorResult>().ToList();
            }

            ParseResults(records);
        }

        //private static void ParseResults(List<CompetitorResult> records)
        //{
        //    coursePunches = new List<CoursePunch>();
        //    foreach (var record in records)
        //    {
        //        bool isControlNumber = false;
        //        foreach (var item in record.ControlPunches)
        //        {
        //            var coursePunch = new CoursePunch();
        //            if (!isControlNumber)
        //            {
        //                coursePunch.CoursePunchTime = item;
        //                isControlNumber = true;
        //            }
        //            else
        //            {
        //                coursePunch.CoursePunchName = item;
        //                isControlNumber = false;
        //                coursePunches.Add(coursePunch);
        //            }
        //        }
        //    }
        //}

        private static void ParseResults(List<CompetitorResult> records)
        {
            //let's parse the results

           
            foreach (var record in records)
            {
                coursePunches = new List<CoursePunch>();
                var startPunch = new CoursePunch
                {
                    SI = record.SI,
                    CoursePunchName = "S",
                    CoursePunchTime = record.StartPunchTime
                };

                coursePunches.Add(startPunch);

                bool isControlNumber = false;
                var coursePunch = new CoursePunch();

                foreach (var item in record.ControlPunches)
                {
                    coursePunch.SI = record.SI;

                    if (!isControlNumber)
                    {
                        coursePunch.CoursePunchTime = item;
                        isControlNumber = true;
                    }
                    else
                    {
                        coursePunch.CoursePunchName = item;
                        isControlNumber = false;
                        coursePunches.Add(coursePunch);
                        coursePunch = new CoursePunch();
                    }
                }
            }

            //adjust the last record
            var lastPunch = coursePunches.LastOrDefault();
            lastPunch.CoursePunchName = "F";

        }

        private static void ParsePunch(string item)
        {
            var controlPunch = new ControlPunch()
            {
                EventId = Convert.ToInt16(ParseEventId(item)),
                PunchDateTime = Convert.ToDateTime(ParseControlPunch(item))
            };
        }

        private static DateTime ParseControlPunch(string item)
        {
            int index = item.IndexOf(':');
            string theDateStr = item.Substring(index + 1, item.Length - 2);
            DateTime dt = ParseDateTime(theDateStr);
            return dt;
        }

        private static DateTime ParseDateTime(string dateTime)
        {
            var today = DateTime.Today;            

            dateTime = ReplaceFirstOccurence(":", " ", dateTime);

            var dt = DateTime.ParseExact(dateTime, "ddd HH:mm:ss",
                               CultureInfo.InvariantCulture);

            //int index = dateTime.IndexOf(':');
            //DateTime newDate = new DateTime();
            //newDate = Convert.ToDateTime(dateTime.Substring(0, index));
            return dt;
        }

        private static string ParseDay(string dateTime)
        {
            //var dayLong = ParseDay(dateTime);
            


            string dayLong = string.Empty;
            int index = dateTime.IndexOf(':');
            string dayShort = dateTime.Substring(0, index);
            string theRestOfTheString = dateTime.Substring(index, dateTime.Length - dayShort.Length);
            switch (dayShort)
            {
                case "Sun":
                    dayLong = "Sunday";
                    //DateTime theDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.DayOfWeek);
                    break;

                case "Mon":
                    dayLong = "Monday";
                    break;

                case "Tue":
                    dayLong = "Tuesday";
                    break;

                case "Wed":
                    dayLong = "Wednesday";
                    break;

                case "Thu":
                    dayLong = "Thursday";
                    break;

                case "Fri":
                    dayLong = "Friday";
                    break;
                case "Sat":
                    dayLong = "Saturday";
                    break;

                default:
                    dayLong = "";
                    break;
            }
            

            return dayLong;
        }

        private static string ReplaceFirstOccurence(string wordToReplace, string replaceWith, string input)
        {
            Regex r = new Regex(wordToReplace, RegexOptions.IgnoreCase);
            return r.Replace(input, replaceWith, 1);
        }

        private static string ParseEventId(string item)
        {
            int index = item.IndexOf(':');
            return item.Substring(0, index);
        }
    }

    class CompetitorResult
    {
        public int SI { get; set; }
        public string Punch1 { get; set; }
        public string Punch2 { get; set; }
        public string StartPunchTime { get; set; }
        public List<string> ControlPunches { get; set; }

    }

    class CompetitorResultSummary
    {
        public int SI { get; set; }
        public int Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public TimeSpan? ElapsedTime { get; set; }
        public int Score { get; set; }
    }

    class ControlPunch
    {
        public int EventId { get; set; }
        public DateTime PunchDateTime { get; set; }
    }

    class CoursePunch
    {
        public int SI { get; set; }
        public string CoursePunchName { get; set; }
        public string CoursePunchTime { get; set; }
    }

    class Control
    {
        public string Name { get; set; }
        public int Field1 { get; set; }
        public int Field2 { get; set; }
        public int Score { get; set; }
        public string Field3 { get; set; }
        public bool Field4 { get; set; }
    }

    enum Status
    {
        NotStarted,
        Started,
        Finished,
        DidNotStart,
        DidNotFinish,
        Mispunch
    }
}
