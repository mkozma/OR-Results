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

        private static FileSystemWatcher watcher;

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

            // Create a new FileSystemWatcher and set its properties.
            using (watcher = new FileSystemWatcher(Settings.DataPath))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess;

                watcher.Filter = Constants.CSV_FILE_TYPE;

                watcher.Changed += OnChanged;

                watcher.EnableRaisingEvents = true;

                Console.WriteLine("OR Results Display  (Press 'q' to quit the sample.)");
                Console.WriteLine("Program launched:" + DateTime.Now.ToString(("dd/MM/yyyy hh:mm")));
                Console.WriteLine("Waiting for changes...");
                while (Console.Read() != 'q') ;
            }
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now} File: {e.Name} {e.ChangeType}");
            EventDayMontitoring();
        }

        private static void Initialise()
        {
            Settings = new Settings();
            ReadSetupFiles();
        }

        private static void ReadSetupFiles()
        {
            competition = new CSVHelper<Competition>().ReadData(Settings.FullPath + Constants.COMPETITION_FILE, new Competition(), ";").ToList();

            Settings.ZeroTime = Shared.GetTimeFromMilliseconds( competition[0].ZeroTime);
            controls = new CSVHelper<Control>().ReadData(Settings.FullPath + Constants.CONTROLS_FILE, new Control()).ToList();
            courses = new CSVHelper<Course>().ReadData(Settings.FullPath + Constants.COURSES_FILE, new Course()).ToList();

            courseVariants = new CSVHelper<CourseVariant>().ReadData(Settings.FullPath + Constants.COURSE_VARIANTS_FILE, new CourseVariant()).ToList();
        }

        private static void EventDayMontitoring()
        {
            BuildInitialiseFile();

            if (Shared.ResultsFileExists())
            {
                results = new CSVHelper<CompetitorResult>().ReadData(Settings.FullPath + Constants.RESULTS_FILE, new CompetitorResult(), Constants.SEMICOLON).ToList();
                ParseResults(results);
                ManipulateData();
                GenerateResults();
            }

            PerformResults();
        }

        private static void BuildInitialiseFile()
        {
            competitors = new CSVHelper<Competitor>().ReadData(Settings.FullPath + Constants.COMPETITORS_FILE, new Competitor(), Constants.SEMICOLON).ToList();
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

        //private static void CheckForStart()
        //{
        //    foreach (var competitorPunches in coursePunches)
        //    {
        //        var competitorCourseSummary = new CompetitorResultSummary();
        //        competitorCourseSummary.SI = competitorPunches.SI;

        //        competitorCourseSummary.CourseId =
        //            (GetCompetitorCourse(competitorCourseSummary) == null)
        //            ? string.Empty
        //            : GetCompetitorCourse(competitorCourseSummary);

        //        competitorCourseSummary.StartTime = competitorPunches.CompetitorControls[0].CoursePunchTime;
        //        if (competitorCourseSummary.StartTime == TimeSpan.Zero)
        //            competitorCourseSummary.Status = (int)Status.DidNotStart;
        //        else
        //        {
        //            if (competitorPunches.CompetitorControls.Any(s => s.CoursePunchName == Constants.FINISH_PUNCH))
        //            {
        //                competitorCourseSummary.FinishTime = competitorPunches.CompetitorControls.SingleOrDefault(s => s.CoursePunchName == Constants.FINISH_PUNCH).CoursePunchTime;
        //                competitorCourseSummary.Status = (int)Status.Finished;

        //                CalculateScore(competitorPunches.CompetitorControls, competitorCourseSummary);
        //            }
        //            else
        //            {
        //                GetStatus(competitorCourseSummary);
        //            }
        //        }
        //        CompetitorCourseSummaries.Add(competitorCourseSummary);
        //    }
        //}

        private static void SortResults()
        {
            CompetitorCourseSummaries = CompetitorCourseSummaries
                .OrderBy(c=>c.CourseId)
                .ThenBy(c=>c.Status)
                .ThenByDescending(c => c.Score)
                .ThenBy(c => c.ElapsedTime)
                .ThenBy(c => Shared.GetName(c.SI))
                .ToList();
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
                var coursePunchSI = competitorPunches.SI;
                var competitorCourseSummary = CompetitorCourseSummaries.FirstOrDefault(c => c.SI == coursePunchSI);

                var hasStart = competitorPunches.CompetitorControls.Any(s => s.CoursePunchName == Constants.START_PUNCH);

                if (!hasStart)
                    competitorCourseSummary.Status = (int)Status.DidNotStart;
                else
                {
                    var courseVariantlastControl = GetCourseLastControlName(competitorCourseSummary.CourseId);

                    //compare the last course variant control to the competitors last control
                    if (competitorPunches.CompetitorControls.Any(s => s.CoursePunchName == Constants.FINISH_PUNCH))
                    {
                        competitorCourseSummary.StartTime = competitorPunches.CompetitorControls[0].CoursePunchTime;
                        competitorCourseSummary.FinishTime = competitorPunches.CompetitorControls.SingleOrDefault(s => s.CoursePunchName == Constants.FINISH_PUNCH).CoursePunchTime;
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
            {
                competitorCourseSummary.Score = CalculateScoreCoursePoints(competitorPunches);
                competitorCourseSummary.Penalty = CheckForPenalties(competitorCourseSummary);
            }
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
                if (!new[] { Constants.START_PUNCH, Constants.FINISH_PUNCH }.Contains(controlPunch))
                {
                    newList.Add(controlPunch);
                }
            }
            return newList;
        }

        private static bool IsScoreCourse(string courseId)
        {
            if (courses.First(c => c.CourseId == courseId).CourseType == Constants.COURSE_TYPE_SCORE)
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

            //CheckForPenalties(coursePunches);

            return amount;
        }

        private static int CheckForPenalties(CompetitorResultSummary competitorResultSummary)
        {
            var courseTimeLimit = courses.FirstOrDefault(c => c.CourseId == competitorResultSummary.CourseId).TimeLimit;
            var penaltyPointsPerMinute = courses.FirstOrDefault(c => c.CourseId == competitorResultSummary.CourseId).PenaltyPointsPerMinute;

            double timeSpanMinutes = competitorResultSummary.ElapsedTime.Value.TotalMinutes;

            var x = (int)Math.Ceiling(timeSpanMinutes);// - courseTimeLimit
            var y = x -  Convert.ToInt16( courseTimeLimit);

            return (y > 0) ? (y * Convert.ToInt16(penaltyPointsPerMinute)) : 0;

            //return 0;

        }

        private static void CalculateElapsedTime(CompetitorResultSummary competitorCourseSummary)
        {
                competitorCourseSummary.ElapsedTime = competitorCourseSummary.FinishTime - competitorCourseSummary.StartTime;
        }

        private static void DisplayResults()
        {
            var displayResults = new DisplayResults(new Settings());
        }

        private static void ParseResults(List<CompetitorResult> records)
        {
            //let's parse the results
           
            coursePunches = new List<CoursePunch>();

            foreach (var record in records)
            {
                if (!isValidSI(record.SI))
                {
                    break;
                }

                var coursePunch = new CoursePunch();
                coursePunch.SI = record.SI;
                var competitorControls = new List<CompetitorControl>();
                var competitorControl = new CompetitorControl();

                var course = Shared.GetCourseBySI(record.SI);
                if(course == null)
                {
                    continue;
                }
                var courseStartTime = Convert.ToInt64(course.CourseStartTime);

                var useCourseTime = false;
                var recordStartTime = record.StartPunchTime;
                var courseStartTimeAsTimeSpan = TimeSpan.MinValue;

                if (recordStartTime == Constants.NULL_RECORD)
                {
                    courseStartTimeAsTimeSpan = Shared.GetTimeFromMilliseconds((long)courseStartTime);
                    useCourseTime = true;
                }

                var startCompetitorControl = new CompetitorControl
                {
                    CoursePunchName = Constants.START_PUNCH,
                    CoursePunchTime = (useCourseTime)? courseStartTimeAsTimeSpan :  ParseControlPunch(recordStartTime)
                };
                competitorControls.Add(startCompetitorControl);

                var finishCompetitorControl = new CompetitorControl
                {
                    CoursePunchName = Constants.FINISH_PUNCH,
                    CoursePunchTime = ParseControlPunch(record.FinishPunchTime)
                };
                competitorControls.Add(finishCompetitorControl);


                foreach (var row in record.ControlPunches)
                {
                    int number;
                    if (Int32.TryParse(row, out number)== true)
                    {
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

        private static bool isValidSI(int sI)
        {
            //throw new NotImplementedException();
            //check result SI entered 
            return competitors.Any(c => c.SI == sI);
        }

        private static TimeSpan ParseControlPunch(string item)
        {
            int index = item.IndexOf(Constants.COLON);
            string theDateStr = item.Substring(index + 1, item.Length - 2);

            return ParseDateTime(item);
        }

        private static string ParseControlPunchName(string item)
        {
            return string.Empty;
        }

        private static TimeSpan ParseDateTime(string dateTime)
        {

            if (dateTime == Constants.NULL_RECORD)
            {
                //check for mass start time

                return new TimeSpan(0, 0, 0);
            }

            var index = dateTime.IndexOf(Constants.COLON);
            var course = dateTime.Substring(0, index);
            var theRest = dateTime.Substring(index + 1, dateTime.Length-course.Length - 1);

            index = theRest.IndexOf(Constants.COLON);
            var day = theRest.Substring(0, index);
            theRest = theRest.Substring(index + 1, theRest.Length- day.Length-1);

            index = theRest.IndexOf(Constants.COLON);
            var hours = theRest.Substring(0, index);
            theRest = theRest.Substring(index + 1, theRest.Length - hours.Length - 1);

            index = theRest.IndexOf(Constants.COLON);
            var minutes = theRest.Substring(0, index);
            var seconds = theRest.Substring(index + 1, theRest.Length - minutes.Length - 1);

            return new TimeSpan(Convert.ToInt16(hours), Convert.ToInt16(minutes), Convert.ToInt16(seconds));
        }
        
    }


}
