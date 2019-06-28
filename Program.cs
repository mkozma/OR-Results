using CsvHelper;
using HtmlAgilityPack;
//using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace OR_Results
{
    class Program
    {
        private static List<CompetitorResult> records;
        private static List<CoursePunch> coursePunches;
        private static List<Control> controls;
        private static List<Competitor> competitors;
        private static List<Course> courses;
        private static List<CourseVariant> courseVariants;
        private static List<Competition> competition;

        private static List<CompetitorResultSummary> competitorCourseSummaries;
        private static string Course;

        private const string DATA_PATH = @"C:\Users\mkozm\Or\21\";

        public static List<CompetitorResultSummary> CompetitorCourseSummaries { get; set; }

        static void Main(string[] args)
        {
            Initialise();
            ParseResults(records);
            ManipulateData();
            PerformResults();
        }

        private static void ManipulateData()
        {
            RemoveSequentialPunches();

            
        }

        private static void RemoveSequentialPunches()
        {
            foreach (var competitor in coursePunches)
            {
                var newControlPuches = new List<CompetitorControl>();
                foreach (var competitorControl in competitor.CompetitorControls)
                {
                    if ((newControlPuches.Count == 0) || (newControlPuches.Last().CoursePunchName != competitorControl.CoursePunchName))
                    {
                        newControlPuches.Add(competitorControl);
                    }
                }
                competitor.CompetitorControls = newControlPuches;
            }
        }

        private static void Initialise()
        {
            competition = new CSVHelper<Competition>().ReadData(DATA_PATH + "competition.csv", new Competition(),";").ToList();
            controls = new CSVHelper<Control>().ReadData(DATA_PATH + "controls.csv", new Control()).ToList();
            competitors = new CSVHelper<Competitor>().ReadData(DATA_PATH + "competitors.csv", new Competitor(), ";").ToList();
            courses = new CSVHelper<Course>().ReadData(DATA_PATH + "courses.csv", new Course()).ToList();

            courseVariants = new CSVHelper<CourseVariant>().ReadData(DATA_PATH + "courseVariants.csv", new CourseVariant()).ToList();

            CompetitorCourseSummaries = new List<CompetitorResultSummary>();

            records = new CSVHelper<CompetitorResult>().ReadData(DATA_PATH + "results.csv", new CompetitorResult(), ";").ToList();
        }

        private static void PerformResults()
        {
            GenerateResults();
            //CalculateResults(); //MK An attempt to simplify
            SortResults();
            DisplayResults();
        }

        private static void CalculateResults()
        {
            //Check for a start
            //If a start check for a finish
            //if a finish
            // Calculate elapsed time
            //If a score course, calculate score
            //If a line course, check for correct punch order
            CheckForStart();
            CheckElapsedTime();
            //ScoreControl();

        }

        //private static void ScoreControl()
        //{
        //    foreach (var competitorCourseSummary in CompetitorCourseSummaries)
        //    {
        //        //get class
        //        competitorCourseSummary.CourseId = GetCompetitorCourse(competitorCourseSummary);
        //        if ((IsScoreCourse(competitorCourseSummary.CourseId)) && (competitorCourseSummary.FinishTime != null))
        //            competitorCourseSummary.Score = CalculateScoreCoursePoints(competitorPunches);

        //    }
        //}

        private static void CheckElapsedTime()
        {
            foreach (var competitorCourseSummary in CompetitorCourseSummaries)
            {
               if (competitorCourseSummary.Status == (int)Status.Finished)
                    CalculateElapsedTime(competitorCourseSummary);
            }
        }

        private static void CheckForStart()
        {
            foreach (var competitorPunches in coursePunches)
            {
                var competitorCourseSummary = new CompetitorResultSummary();
                competitorCourseSummary.SI = competitorPunches.SI;

                competitorCourseSummary.CourseId =
                    (GetCompetitorCourse(competitorCourseSummary) == null)
                    ? string.Empty
                    : GetCompetitorCourse(competitorCourseSummary);

                competitorCourseSummary.StartTime = competitorPunches.CompetitorControls[0].CoursePunchTime;
                if (competitorCourseSummary.StartTime == TimeSpan.Zero)
                {
                    competitorCourseSummary.Status = (int)Status.DidNotStart;
                }
                else
                {
                    if (competitorPunches.CompetitorControls.Any(s => s.CoursePunchName == "F"))
                    {
                        competitorCourseSummary.FinishTime = competitorPunches.CompetitorControls.SingleOrDefault(s => s.CoursePunchName == "F").CoursePunchTime;
                        competitorCourseSummary.Status = (int)Status.Finished;
                        //CalculateElapsedTime(competitorCourseSummary);

                        CalculateScore(competitorPunches.CompetitorControls, competitorCourseSummary);
                    }
                    else
                    {
                        GetStatus(competitorCourseSummary);
                    }
                }
                CompetitorCourseSummaries.Add(competitorCourseSummary);
            }
        }

        private static void SortResults()
        {
            CompetitorCourseSummaries = CompetitorCourseSummaries
                .OrderBy(c=>c.CourseId)
                .ThenBy(c=>c.Status)
                .ThenByDescending(c => c.Score)
                .ThenBy(c => c.ElapsedTime)
                .ToList();

            PrintResults();
        }

        private static void PrintResults()
        {
            var score = string.Empty;
            var courseCount = 0;

            var prevCourse = string.Empty;
            for (int i = 1; i<= CompetitorCourseSummaries.Count; i++)
            {
                courseCount = (prevCourse == CompetitorCourseSummaries[i - 1].CourseId) ? ++courseCount :  1;
                Console.Write("{0} {1} {2} {3} {4} {5} {6} {7}",
                    i.ToString(),
                    courseCount.ToString(),
                    CompetitorCourseSummaries[i - 1].CourseId,
                    GetEnumValue(CompetitorCourseSummaries[i - 1].Status),
                    CompetitorCourseSummaries[i - 1].SI,
                    GetName(CompetitorCourseSummaries[i - 1].SI),
                    score = (CompetitorCourseSummaries[i - 1].Score == 0) ? string.Empty : CompetitorCourseSummaries[i - 1].Score.ToString(),
                    CompetitorCourseSummaries[i-1].ElapsedTime.ToString());
                Console.WriteLine();
                prevCourse = CompetitorCourseSummaries[i - 1].CourseId;
            }
            Console.ReadLine();
        }

        private static string GetEnumValue(int value)
        {
            Status enumDisplayStatus = (Status)value;
            return enumDisplayStatus.ToString();
        }

        private static object GetName(int sI)
        {
            return competitors.FirstOrDefault(c => c.SI == sI).Name;
        }

        private static void GenerateResults()
        {
            foreach (var competitorPunches in coursePunches)
            {
                var competitorCourseSummary = new CompetitorResultSummary();
                competitorCourseSummary.SI = competitorPunches.SI;

                competitorCourseSummary.CourseId =
                    (GetCompetitorCourse(competitorCourseSummary) == null)
                    ? string.Empty
                    : GetCompetitorCourse(competitorCourseSummary);

                competitorCourseSummary.StartTime = competitorPunches.CompetitorControls[0].CoursePunchTime;
                if(competitorCourseSummary.StartTime == TimeSpan.Zero)
                {
                    competitorCourseSummary.Status = (int)Status.DidNotStart;
                }
                else
                {
                    if (competitorPunches.CompetitorControls.Any(s => s.CoursePunchName == "F"))
                    {
                        competitorCourseSummary.FinishTime = competitorPunches.CompetitorControls.SingleOrDefault(s => s.CoursePunchName == "F").CoursePunchTime;
                        competitorCourseSummary.Status = (int)Status.Finished;
                        CalculateElapsedTime(competitorCourseSummary);

                        CalculateScore(competitorPunches.CompetitorControls, competitorCourseSummary);
                    }
                    else
                    {
                        GetStatus(competitorCourseSummary);
                    }
                }
                CompetitorCourseSummaries.Add(competitorCourseSummary);
            }
        }

        private static void GetStatus(CompetitorResultSummary competitorCourseSummary)
        {
            competitorCourseSummary.FinishTime = null;

            competitorCourseSummary.Status = (int)Status.Started;
        }

        private static void CalculateScore(List<CompetitorControl> competitorPunches, CompetitorResultSummary competitorCourseSummary)
        {
            competitorCourseSummary.CourseId = GetCompetitorCourse(competitorCourseSummary);
            if ((IsScoreCourse(competitorCourseSummary.CourseId)) && (competitorCourseSummary.FinishTime != null))
                competitorCourseSummary.Score = CalculateScoreCoursePoints(competitorPunches);
            else
                competitorCourseSummary.Status = CheckLineCourse(competitorCourseSummary.CourseId, competitorPunches);
        }

        private static int CheckLineCourse(string courseId, List<CompetitorControl> competitorPunches)
        {
            var courseVariantList = courseVariants.FirstOrDefault(c => c.CourseId == courseId).Controls.ToList();
            courseVariantList.RemoveAt(0);
            courseVariantList.Reverse();
            courseVariantList.RemoveAt(0);
            courseVariantList.Reverse();


            var competitorPunchesList = competitorPunches.Select(c => c.CoursePunchName).ToList();
            var validCompetitorPunches = CheckForValidPunches(competitorPunchesList);

            var isEqual = Enumerable.SequenceEqual(courseVariantList, validCompetitorPunches);

            if (!isEqual)
                return (int)Status.Mispunch;

            return (int)Status.Finished;
            
        }

        private static List<string> CheckForValidPunches(List<string> competitorPunchesList)
        {
            var newList = new List<string>();
            foreach (var controlPunch in competitorPunchesList)
            {
                if (!new[] { "S", "F" }.Contains(controlPunch))
                {
                    newList.Add(controlPunch);
                }
            }
            return newList;
        }

        private static bool IsScoreCourse(string courseId)
        {
            if (courses.First(c => c.CourseId == courseId).CourseType == "Score")
                return true;
            return false;
        }

        private static string GetCompetitorCourse(CompetitorResultSummary competitorCourseSummary)
        {
            return competitors.FirstOrDefault(c => c.SI == competitorCourseSummary.SI).CourseId;
        }

        private static int CalculateScoreCoursePoints(List<CompetitorControl> coursePunches)
        {
            var amount = 0;
            foreach (var competitorControl in coursePunches)
            {
                var control = controls.FirstOrDefault(s => s.Name == competitorControl.CoursePunchName);
                amount += (control == null) ? 0 : control.Score;
            }
            return amount;
        }

        private static void CalculateElapsedTime(CompetitorResultSummary competitorCourseSummary)
        {
                competitorCourseSummary.ElapsedTime = competitorCourseSummary.FinishTime - competitorCourseSummary.StartTime;
        }

        private static void DisplayResults()
        {
            var doc = new HtmlDocument();
            var html = new StringBuilder();
          
            html.Append("<html>");
            html.Append("<head>");
            html.Append("</head>");
            html.Append("<body>");
            html.Append("<h1>OR Results</h1>");
            html.Append("<script src='js/jquery.min.js'></script>");
            html.Append("<script src='js/bootstrap.min.js'></script>");
            html.Append("</body>");
            html.Append("</html>");

            doc.LoadHtml(html.ToString());

            FileStream sw = new FileStream("FileStream.html", FileMode.Create);

            var currentDir = @"C:\inetpub\wwwroot" + @"\";

            var path = currentDir + "FileStream.html";

            doc.Save(path);
            System.Diagnostics.Process.Start(path);
        }

        private static void ParseResults(List<CompetitorResult> records)
        {
            //let's parse the results
           
            coursePunches = new List<CoursePunch>();

            foreach (var record in records)
            {
                var coursePunch = new CoursePunch();
                coursePunch.SI = record.SI;
                var competitorControls = new List<CompetitorControl>();
                var competitorControl = new CompetitorControl();

                var startCompetitorControl = new CompetitorControl
                {
                    CoursePunchName = "S",
                    CoursePunchTime = ParseControlPunch(record.StartPunchTime)
                };
                competitorControls.Add(startCompetitorControl);

                var finishCompetitorControl = new CompetitorControl
                {
                    CoursePunchName = "F",
                    CoursePunchTime = ParseControlPunch(record.FinishPunchTime)
                };
                competitorControls.Add(finishCompetitorControl);


                foreach (var row in record.ControlPunches)
                {
                        int number;
                        if (Int32.TryParse(row, out number)== true)
                        {
                            competitorControl.CoursePunchName = row;
                            competitorControl = new CompetitorControl{  CoursePunchName = row };
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(row))
                            {
                                competitorControl.CoursePunchTime = ParseControlPunch(row);
                                competitorControls.Add(competitorControl);
                            }
                        }
                }
                coursePunch.CompetitorControls = competitorControls;

                coursePunches.Add(coursePunch);
            }
        }

        private static TimeSpan ParseControlPunch(string item)
        {
            int index = item.IndexOf(':');
            string theDateStr = item.Substring(index + 1, item.Length - 2);

            return ParseDateTime(item);
        }

        private static TimeSpan ParseDateTime(string dateTime)
        {
            char semiColon = ':';
            char hyphen = '-';

            if (dateTime.Contains(hyphen) == true) { return new TimeSpan(0, 0, 0); }

            var index = dateTime.IndexOf(semiColon);
            var course = dateTime.Substring(0, index);
            var theRest = dateTime.Substring(index + 1, dateTime.Length-course.Length - 1);

            index = theRest.IndexOf(semiColon);
            var day = theRest.Substring(0, index);
            theRest = theRest.Substring(index + 1, theRest.Length- day.Length-1);

            index = theRest.IndexOf(semiColon);
            var hours = theRest.Substring(0, index);
            theRest = theRest.Substring(index + 1, theRest.Length - hours.Length - 1);

            index = theRest.IndexOf(semiColon);
            var minutes = theRest.Substring(0, index);
            var seconds = theRest.Substring(index + 1, theRest.Length - minutes.Length - 1);

            return new TimeSpan(Convert.ToInt16(hours), Convert.ToInt16(minutes), Convert.ToInt16(seconds));
        }

        private static string ParseEventId(string item)
        {
            int index = item.IndexOf(':');
            return item.Substring(0, index);
        }
    }

    class CoursePunch
    {
        public int SI { get; set; }
        public List<CompetitorControl> CompetitorControls { get; set; }
    }

    class CompetitorControl
    {
        public string CoursePunchName { get; set; }
        public TimeSpan CoursePunchTime { get; set; }
    }

    enum Status
    {
        Finished,
        Started,
        Mispunch,
        DidNotFinish,
        DidNotStart
    }
   
}
