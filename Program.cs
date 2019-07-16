using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;

namespace OR_Results
{
    class Program
    {
        public static List<CompetitorResult> results;
        public static List<CoursePunch> coursePunches;
        public static List<Control> controls;
        public static List<Competitor> competitors;
        public static List<Course> courses;
        public static List<CourseVariant> courseVariants;
        public static List<Competition> competition;

        public static List<CompetitorResultSummary> competitorCourseSummaries;
        private static string Course;
        private static FileSystemWatcher watcher;

        //private const string DATA_PATH = @"C:\Users\mkozm\Or\21\";

        public static List<CompetitorResultSummary> CompetitorCourseSummaries { get; set; }

        public static Settings Settings { get; set; }

        static void Main(string[] args)
        {
            Initialise();
            EventDayMontitoring();
            SetFileWatcher();

        }


        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private static void SetFileWatcher()
        {
            string[] args = Environment.GetCommandLineArgs();

            // If a directory is not specified, exit program.
            //if (args.Length != 2)
            //{
            //    // Display the proper way to call the program.
            //    Console.WriteLine("Usage: Watcher.exe (directory)");
            //    return;
            //}

            // Create a new FileSystemWatcher and set its properties.
            using (watcher = new FileSystemWatcher(Settings.DataPath))
            {
                //watcher.Path = args[1];

                // Watch for changes in LastAccess and LastWrite times, and
                // the renaming of files or directories.
                watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess;

                watcher.Filter = "*.csv";

                // Add event handlers.
                watcher.Changed += OnChanged;

                // Begin watching.
                watcher.EnableRaisingEvents = true;

                // Wait for the user to quit the program.
                Console.WriteLine("OR Results Display  (Press 'q' to quit the sample.)");
                while (Console.Read() != 'q') ;
            }
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine($"File: {e.Name} {e.ChangeType}");
            //watcher.EnableRaisingEvents = false;
            EventDayMontitoring();
        }

        private static void Initialise()
        {
            Settings = new Settings();
            ReadSetupFiles();
            //BuildInitialiseFile();
            //PerformResults();

        }

        private static void ReadSetupFiles()
        {
            competition = new CSVHelper<Competition>().ReadData(Settings.FullPath + "competition.csv", new Competition(), ";").ToList();
            controls = new CSVHelper<Control>().ReadData(Settings.FullPath + "controls.csv", new Control()).ToList();
            courses = new CSVHelper<Course>().ReadData(Settings.FullPath + "courses.csv", new Course()).ToList();

            courseVariants = new CSVHelper<CourseVariant>().ReadData(Settings.FullPath + "courseVariants.csv", new CourseVariant()).ToList();
        }

        private static void EventDayMontitoring()
        {
            BuildInitialiseFile();

            if (Shared.ResultsFileExists())
            {
                results = new CSVHelper<CompetitorResult>().ReadData(Settings.FullPath + "results.csv", new CompetitorResult(), ";").ToList();
                ParseResults(results);
                ManipulateData();
                GenerateResults();
            }

            PerformResults();
        }

        private static void BuildInitialiseFile()
        {
            competitors = new CSVHelper<Competitor>().ReadData(Settings.FullPath + "competitors.csv", new Competitor(), ";").ToList();
            if (competitors == null)
                return;

            CompetitorCourseSummaries = new List<CompetitorResultSummary>();

            foreach(var competitor in competitors)
            {
                var competitorResultSummary = new CompetitorResultSummary
                {
                    SI = competitor.SI,
                    CourseId = competitor.CourseId,
                    ClassId = Shared.GetCompetitorClass(competitor.SI),
                    Status = (int)Status.DidNotStart,
                    ElapsedTime = TimeSpan.MaxValue
                    
                };
                CompetitorCourseSummaries.Add(competitorResultSummary);
            }
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
        private static void PerformResults()
        {
            SortResults();
            DisplayResults();
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
                    competitorCourseSummary.Status = (int)Status.DidNotStart;
                else
                {
                    if (competitorPunches.CompetitorControls.Any(s => s.CoursePunchName == "F"))
                    {
                        competitorCourseSummary.FinishTime = competitorPunches.CompetitorControls.SingleOrDefault(s => s.CoursePunchName == "F").CoursePunchTime;
                        competitorCourseSummary.Status = (int)Status.Finished;

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
                .ThenBy(c => Shared.GetName(c.SI))
                .ToList();

            //PrintResults();
        }

        private static void PrintResults()
        {
            var score = string.Empty;
            var courseCount = 0;
            var classCount = 0;
            var elapsedTime = string.Empty;
            var className = string.Empty;


            var prevCourse = string.Empty;
            var prevClass = string.Empty;

            var scorePts = string.Empty;

            for (int i = 1; i<= CompetitorCourseSummaries.Count; i++)
            {
                courseCount = (prevCourse == CompetitorCourseSummaries[i - 1].CourseId) ? ++courseCount :  1;
                classCount = (prevClass == GetCompetitorClass(CompetitorCourseSummaries[i - 1])) ? ++classCount : 1;
                Console.Write("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}",
                    i.ToString(),
                    courseCount.ToString(),
                    classCount.ToString(),
                    CompetitorCourseSummaries[i - 1].CourseId,
                    className = GetCompetitorClass(CompetitorCourseSummaries[i - 1]),
                    GetEnumValue(CompetitorCourseSummaries[i - 1].Status),
                    CompetitorCourseSummaries[i - 1].SI,
                    GetName(CompetitorCourseSummaries[i - 1].SI),
                    score = (CompetitorCourseSummaries[i - 1].Score == 0) ? string.Empty : CompetitorCourseSummaries[i - 1].Score.ToString(),
                    elapsedTime = (CompetitorCourseSummaries[i - 1].Status == (int)Status.Finished) ?
                    CompetitorCourseSummaries[i - 1].ElapsedTime.ToString() : string.Empty);
                Console.WriteLine();
                prevCourse = CompetitorCourseSummaries[i - 1].CourseId;
                prevClass = GetCompetitorClass(CompetitorCourseSummaries[i - 1]);

            }
            Console.ReadLine();
        }

        private static string GetEnumValue(int value)
        {
            Status enumDisplayStatus = (Status)value;
            return enumDisplayStatus.ToString();
        }

        public static object GetName(int sI)
        {
            return competitors.FirstOrDefault(c => c.SI == sI).Name;
        }

        private static void GenerateResults()
        {
            foreach (var competitorPunches in coursePunches)
            {
                //var competitorCourseSummary = new CompetitorResultSummary();
                var coursePunchSI = competitorPunches.SI;
                var competitorCourseSummary = CompetitorCourseSummaries.FirstOrDefault(c => c.SI == coursePunchSI);

                var hasStart = competitorPunches.CompetitorControls.Any(s => s.CoursePunchName == "S");

                if (!hasStart)

                //if (competitorCourseSummary.StartTime == TimeSpan.Zero)
                    competitorCourseSummary.Status = (int)Status.DidNotStart;
                else
                {
                    var courseVariantlastControl = GetCourseLastControlName(competitorCourseSummary.CourseId);

                    //compare the last course variant control to the competitors last control
                    if (competitorPunches.CompetitorControls.Any(s => s.CoursePunchName == "F"))
                    {
                        competitorCourseSummary.FinishTime = competitorPunches.CompetitorControls.SingleOrDefault(s => s.CoursePunchName == "F").CoursePunchTime;
                        competitorCourseSummary.FinishTime = competitorPunches.CompetitorControls[1].CoursePunchTime;
                        competitorCourseSummary.Status = (int)Status.Finished;
                        CalculateElapsedTime(competitorCourseSummary);

                        CalculateScore(competitorPunches.CompetitorControls, competitorCourseSummary);
                    }
                    else
                        GetStatus(competitorCourseSummary);
                }
            }
        }

        private static string GetCourseLastControlName(string courseId)
        {
            var courseVariant = courseVariants.FirstOrDefault(c => c.CourseId == courseId);
            courseVariant.Controls.Reverse();
            var lastCourseControlName = courseVariant.Controls[0];
            courseVariant.Controls.Reverse();
            return lastCourseControlName;
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

            var competitorPunchesList = competitorPunches.Select(c => c.CoursePunchName).ToList();
            var validCompetitorPunches = CheckForValidPunches(competitorPunchesList);

            var isValid = true;

            isValid = CompareCoursePunchesToCompetitorPunches(courseVariantList, validCompetitorPunches, isValid);

            if (!isValid)
                return (int)Status.Mispunch;

            return (int)Status.Finished;

        }

        private static bool CompareCoursePunchesToCompetitorPunches(List<string> courseVariantList, List<string> validCompetitorPunches, bool isValid)
        {
            for (int i = 0; i < courseVariantList.Count; i++)
            {
                for (int j = i; j < validCompetitorPunches.Count; j++)
                {
                    if (courseVariantList[i] == validCompetitorPunches[j])
                    {
                        isValid = true;
                        break;
                    }
                    else
                    {
                        isValid = false;
                    }
                }
            }

            return isValid;
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

        public static string GetCompetitorClass(CompetitorResultSummary competitorResultSummary)
        {
            return competitors.FirstOrDefault(c => c.SI == competitorResultSummary.SI).ClassId;
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
            var displayResults = new DisplayResults();
            //watcher.EnableRaisingEvents = true;
            //Console.ReadLine();
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
                            //competitorControl.CoursePunchName = row;
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

        private static string ParseControlPunchName(string item)
        {
            return string.Empty;
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


}
