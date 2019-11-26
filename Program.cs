using OR_Results.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Timers;

namespace OR_Results
{
    class Program
    {
        private static FileSystemWatcher watcher;
        private static Settings _settings;

        public static List<CompetitorResultSummary> CompetitorCourseSummaries { get; set; }
        public static List<CompetitorResult> results;
        public static List<CoursePunch> coursePunches;
        private static CompetitionService competitionService = new CompetitionService();
        private static ControlService controlService = new ControlService();
        private static CourseService courseService = new CourseService();
        private static CourseVariantService courseVariantService = new CourseVariantService();
        private static CompetitorService competitorService = new CompetitorService();

        static void Main(string[] args)
        {
            if (Initialise())
            {
                if (!EventDayMontitoring())
                    return;
                SetFileWatcher();
            }
            else
                Message();
        }

        private static void Message()
        {
            Console.WriteLine("Unable to continue as setup files are not complete.");
            
            Console.ReadLine();
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private static void SetFileWatcher()
        {
            // Create a new FileSystemWatcher and set its properties.
            using (watcher = new FileSystemWatcher(_settings.FullPath))
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

        private static bool Initialise()
        {
            _settings = new Settings();
            Console.WriteLine(Shared.GetVersion());
            InitialiseTimer();

            return ReadSetupFiles();
        }

        private static void InitialiseTimer()
        {
            //throw new NotImplementedException();
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = _settings.TimerInterval;
            aTimer.Enabled = _settings.Timer;
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Timer fired: {0}", DateTime.Now.ToString());
            EventDayMontitoring();
        }

        private static bool ReadSetupFiles()
        {
            var validSetupFiles = true;

            //COMPETITION
            var competitionFilenameAndPath = _settings.FullPath + Constants.COMPETITION_FILE;
            validSetupFiles = (Shared.IsFileExists(competitionFilenameAndPath));
            if (!validSetupFiles)
            {
                Console.WriteLine("Error - Invalid setup file");
                return false;
            }

           competitionService.competition = new CSVHelper<Competition>().ReadData(_settings.FullPath + Constants.COMPETITION_FILE, new Competition(), ";").ToList();

            //CONTROL
            _settings.ZeroTime = Shared.GetTimeFromMilliseconds(competitionService.competition[0].ZeroTime);
            var controlsFilenameAndPath = _settings.FullPath + Constants.CONTROLS_FILE;
            validSetupFiles = (Shared.IsFileExists(controlsFilenameAndPath));
            if (!validSetupFiles) return false;
            controlService.controls  = new CSVHelper<Control>().ReadData(_settings.FullPath + Constants.CONTROLS_FILE, new Control()).ToList();

            //COURSE
            var coursesFilenameAndPath = _settings.FullPath + Constants.COURSES_FILE;
            validSetupFiles = (Shared.IsFileExists(coursesFilenameAndPath));
            if (!validSetupFiles) return false;
            courseService.courses = new CSVHelper<Course>().ReadData(_settings.FullPath + Constants.COURSES_FILE, new Course()).ToList();

            //COURSEVARIANT
            var coursesVariantsFilenameAndPath = _settings.FullPath + Constants.COURSE_VARIANTS_FILE;
            validSetupFiles = (Shared.IsFileExists(coursesVariantsFilenameAndPath));
            if (!validSetupFiles) return false;
             courseVariantService.courseVariants = new CSVHelper<CourseVariant>().ReadData(_settings.FullPath + Constants.COURSE_VARIANTS_FILE, new CourseVariant()).ToList();

            return validSetupFiles;
        }

        private static bool Validate()
        {
            //check all competitor courses match courses
            foreach(var competitor in competitorService.GetAll())
            {
                if (!courseService.CompetitorCourseExists(competitor.CourseId))
                    return false;
            }

            return true;
        }

        private static bool EventDayMontitoring()
        {
            if (!BuildInitialiseFile())
            {
                Console.WriteLine("Error - file not found or competitor course not match courses in courses file");
                Console.ReadLine();
                return false;
            }

            if (Shared.ResultsFileExists())
            {
                  results = new CSVHelper<CompetitorResult>().ReadData(_settings.FullPath + Constants.RESULTS_FILE, new CompetitorResult(), Constants.SEMICOLON).ToList();
                ParseResults(results);
                ManipulateData();
                GenerateResults();
            }

            PerformResults();

            return true;
        }

        private static bool BuildInitialiseFile()
        {
            // COMPETITOR
            var competitorsFilenameAndPath = _settings.FullPath + Constants.COMPETITORS_FILE;
            if (!Shared.IsFileExists(competitorsFilenameAndPath))
                return false;

            competitorService.competitors = new CSVHelper<Competitor>().ReadData(_settings.FullPath + Constants.COMPETITORS_FILE, new Competitor(), Constants.SEMICOLON).ToList();
            if ((competitorService.competitors == null) || (!Validate()))
                return false;

            BuildCompetitorSummary();

            return true;
        }

        private static void BuildCompetitorSummary()
        {
            CompetitorCourseSummaries = new List<CompetitorResultSummary>();

            foreach (var competitor in competitorService.GetAll())
            {
                var competitorResultSummary = new CompetitorResultSummary
                {
                    SI = competitor.SI,
                    CourseId = competitor.CourseId,
                    ClassId =  competitorService.Get(competitor.SI).ClassId,
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

        private static void SortResults()
        {
            int[] customStatusOrder = new int[] { 0, 4, 1, 2, 3 };

            CompetitorCourseSummaries = CompetitorCourseSummaries
                .OrderBy(c=>c.CourseId)
                .ThenBy(c=>customStatusOrder)
                .ThenBy(c=>c.Status)
                .ThenByDescending(c => c.Score)
                .ThenBy(c => c.ElapsedTime)
                .ThenBy(c => competitorService.Get(c.SI).Name)
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

            for (int i = 1; i <= CompetitorCourseSummaries.Count; i++)
            {
                courseCount = (prevCourse == CompetitorCourseSummaries[i - 1].CourseId) ? ++courseCount : 1;
                classCount = (prevClass == competitorService.Get(CompetitorCourseSummaries[i - 1].SI).ClassId) ? ++classCount : 1;
                Console.Write("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}",
                    i.ToString(),
                    courseCount.ToString(),
                    classCount.ToString(),
                    CompetitorCourseSummaries[i - 1].CourseId,
                    className = competitorService.Get(CompetitorCourseSummaries[i - 1].SI).ClassId,
                    GetEnumValue(CompetitorCourseSummaries[i - 1].Status),
                    CompetitorCourseSummaries[i - 1].SI,
                    competitorService.Get(CompetitorCourseSummaries[i - 1].SI).Name,
                    score = (CompetitorCourseSummaries[i - 1].Score == 0) ? string.Empty : CompetitorCourseSummaries[i - 1].Score.ToString(),
                    elapsedTime = (CompetitorCourseSummaries[i - 1].Status == (int)Status.Finished) ?
                    CompetitorCourseSummaries[i - 1].ElapsedTime.ToString() : string.Empty);

                Console.WriteLine();
                prevCourse = CompetitorCourseSummaries[i - 1].CourseId;
                prevClass = competitorService.GetCompetitorClass(CompetitorCourseSummaries[i - 1]);

            }
            Console.ReadLine();
        }

        private static string GetEnumValue(int value)
        {
            Status enumDisplayStatus = (Status)value;
            return enumDisplayStatus.ToString();
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
            var courseVariant = courseVariantService.Get(courseId);
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
            competitorCourseSummary.CourseId = competitorService.GetCompetitorCourse(competitorCourseSummary);
            if ((courseService.IsScoreCourse(competitorCourseSummary.CourseId)) && (competitorCourseSummary.FinishTime != null))
            {
                competitorCourseSummary.Score = CalculateScoreCoursePoints(competitorPunches);
                competitorCourseSummary.Penalty = CheckForPenalties(competitorCourseSummary);
            }
            else
                competitorCourseSummary.Status = CheckLineCourse(competitorCourseSummary.CourseId, competitorPunches);
        }

        private static int CheckLineCourse(string courseId, List<CompetitorControl> competitorPunches)
        {
            var courseVariantList = courseVariantService.Get(courseId).Controls.ToList();
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

        private static int CalculateScoreCoursePoints(List<CompetitorControl> coursePunches)
        {
            var amount = 0;
            foreach (var competitorControl in coursePunches)
            {
                var control = controlService.controls.FirstOrDefault(s => s.Name == competitorControl.CoursePunchName);
                amount += (control == null) ? 0 : control.Score;
            }

            return amount;
        }

        private static int CheckForPenalties(CompetitorResultSummary competitorResultSummary)
        {
            var course = courseService.Get(competitorResultSummary.CourseId);

            var courseTimeLimit = course.TimeLimit;
            var penaltyPointsPerMinute = course.PenaltyPointsPerMinute;

            double timeSpanMinutes = competitorResultSummary.ElapsedTime.Value.TotalMinutes;

            var x = (int)Math.Ceiling(timeSpanMinutes);
            var y = x -  Convert.ToInt16( courseTimeLimit);

            return (y > 0) ? (y * Convert.ToInt16(penaltyPointsPerMinute)) : 0;

        }

        private static void CalculateElapsedTime(CompetitorResultSummary competitorCourseSummary)
        {
            var ft = competitorCourseSummary.FinishTime.Value;
            if (ft.Hours == 0)
            {
                ft = TimeSpan.FromHours(12);
                competitorCourseSummary.FinishTime = ft;
            }

            competitorCourseSummary.ElapsedTime = competitorCourseSummary.FinishTime - competitorCourseSummary.StartTime;
        }

        private static void DisplayResults()
        {
            var displayResults = new DisplayResults(
                _settings, 
                courseService, 
                competitorService);

            var displayResultsHtml = new DisplayResultsHtml(
                _settings, 
                displayResults.listDisplayTable,
                competitionService,
                courseService,
                competitorService
                );
            displayResultsHtml.BaseHTMLFile();
        }

        private static void ParseResults(List<CompetitorResult> records)
        {
            //let's parse the results
           
            coursePunches = new List<CoursePunch>();

            foreach (var record in records)
            {
                if (!competitorService.isValidSI(record.SI))
                {
                    break;
                }

                var coursePunch = new CoursePunch();
                coursePunch.SI = record.SI;
                var competitorControls = new List<CompetitorControl>();
                var competitorControl = new CompetitorControl();
                
                var course = courseService.GetCourseBySI(record.SI, competitorService);
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
