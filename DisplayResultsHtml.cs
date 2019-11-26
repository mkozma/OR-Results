using HtmlAgilityPack;
using OR_Results.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace OR_Results
{
    public class DisplayResultsHtml
    {
        private readonly Settings _settings;
        private readonly List<DisplayTable> _listDisplayTable;
        private readonly CompetitionService _competitionService;
        private readonly Competition competition;
        private readonly CourseService _courseService;
        private readonly CompetitorService _competitorService;
        HtmlDocument doc = new HtmlDocument();
        StringBuilder html = new StringBuilder();

        public DisplayResultsHtml(Settings settings,
                                  List<DisplayTable> listDisplayTable,
                                  CompetitionService competitionService,
                                  CourseService courseService,
                                  CompetitorService competitorService)
        {
            _settings = settings;
            _listDisplayTable = listDisplayTable;
            _competitionService = competitionService;
            competition = competitionService.GetFirst();
            _courseService = courseService;
            _competitorService = competitorService;
        }
        public void BaseHTMLFile()
        {
            html.Append("<html>");
            html.Append("<head>");
            if (_settings.RefreshBrowser)
                html.Append("<meta http-equiv='refresh' content='5' />");

            html.Append("<Link rel='stylesheet' href='css/bootstrap.min.css'>");
            html.Append("<Link rel='stylesheet' href='css/main.css'>");

            html.Append("</head>");
            html.Append("<body>");

            html.Append("<div class='container-fluid'>");

            html.Append("<div class='row'>");
            html.Append("<div class='col-sm'>");
            html.Append("<h3>");
            html.Append("Last update: " + Shared.GetCurrentTime());
            html.Append("</h3>");
            html.Append("</div>");
            html.Append("<div class='col-sm'>");
            html.Append("<h2>");
            html.Append(competition.Name);
            html.Append("</h2>");
            html.Append("</div>");
            html.Append("<div class='col-sm'>");
            html.Append("<h3>");
            html.Append("Competitors: ");
            html.Append(Program.CompetitorCourseSummaries.Count.ToString());
            html.Append(" ");
            html.Append("Remaining: ");
            html.Append(Shared.NumberOfCompetitorsRemaining(Program.CompetitorCourseSummaries));
            html.Append("</h3>");
            html.Append("</div>");
            html.Append("</div>");

            html.Append("<div class='row'>");
            
            foreach(var displayTable in _listDisplayTable)
            {
                DisplayColumns(displayTable);
            }

            html.Append("</div>");
            html.Append("<div class='row'>");
            html.Append("<div class='col-sm footer'>");
            html.Append(Shared.GetVersion());
            html.Append("</div>");
            html.Append("</div>");

            html.Append("<script src='js/jquery.min.js'></script>");
            html.Append("<script src='js/bootstrap.min.js'></script>");

            html.Append("</body>");

            html.Append("</html>");

            doc.LoadHtml(html.ToString());

            doc.Save(_settings.WebServerFilePath);

            if (_settings.OpenLocalBrowser)
                System.Diagnostics.Process.Start(_settings.WebServerFilePath);
        }

    
        #region Redundant

        private void DisplayColumns(DisplayTable displayTable)
        {
            html.Append("<div class='col-lg'>");
            html.Append("<table class='table'>");

            foreach (var displayCourse in displayTable.ListDisplayCourses)
            {
                var courseDetails = Shared.GetCourseHeader(_listDisplayTable, displayTable, displayCourse);
                html.Append("<thead>");
                html.Append("<tr class='class-header'>");
                html.Append("<th class='course-header' scope='col'>");
                html.Append(courseDetails);
                html.Append("</th>");

                if (_courseService.IsScoreCourse(displayCourse.CourseId))
                    html.Append("<th scope='col'>#</th>");

                html.Append("<th scope='col'>St</th>");
                html.Append("<th scope='col'>Name</th>");
                html.Append("<th scope='col'>Time</th>");

                if (_courseService.IsScoreCourse(displayCourse.CourseId))
                {
                    html.Append("<th scope='col'>Penalty</th>");
                    html.Append("<th scope='col'>Net Score</th>");
                }
                else
                {
                    html.Append("<th scope='col'>Diff</th>");
                }
                html.Append("</tr>");
                html.Append("</thead>");

                foreach (var item in displayCourse.ListDisplayRows)
                {
                    var classGender = string.Empty;
                    if (Shared.GetGenderFromClass(item.Class) == Constants.CLASS_TYPE_MEN)
                        classGender = "men";
                    else
                        classGender = "women";

                    html.Append("<tr");
                    html.Append(" class='");
                    html.Append(classGender);
                    html.Append("'");
                    html.Append(">");

                    html.Append("<th scope ='row'>");
                    html.Append(item.Id);
                    html.Append("</th>");

                    if (_courseService.IsScoreCourse(item.Course))
                    {
                        html.Append("<td class='class'>");
                        html.Append(item.ClassCount);
                        html.Append("</td>");
                    }

                    html.Append("<td class='status'>");
                    html.Append("<img src='Images/");
                    html.Append(Shared.GetEnumValue(item.Status));
                    html.Append("-15");
                    html.Append(".png' class='img-responsive'>");
                    html.Append("</td>");

                    html.Append("<td>");
                    html.Append("<img src='Images/");
                    html.Append(Shared.GetClubImage(Convert.ToInt32(item.SI), _competitorService));
                    html.Append("' class='img-responsive mx-auto'>");
                    html.Append(" ");
                    html.Append(item.Name);
                    html.Append("</td>");

                    html.Append("<td class='elapsed-time'>");
                    var elapsedTime = Shared.GetElapsedTime(item.ElapsedTime, item.Status);
                    html.Append(elapsedTime);
                    html.Append("</td>");

                    if (_courseService.IsScoreCourse(displayCourse.CourseId))
                    {
                        html.Append("<td class='penalty'>");
                        html.Append((item.Penalty == 0) ? string.Empty : item.Penalty.ToString());
                        html.Append("</td>");

                        html.Append("<td class='net-score'>");
                        html.Append((item.NetScore == 0) ? string.Empty : item.NetScore.ToString());
                        html.Append("</td>");
                    }
                    else
                    {
                        html.Append("<td class='time-difference'>");
                        html.Append(item.TimeDifference);
                        html.Append("</td>");
                    }

                    html.Append("</tr>");
                }
            }
            html.Append("</table>");
            html.Append("</div>");
        }

        #endregion
    }
}
