using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OR_Results
{
    public class DisplayResults
    {
        public int mensCount { get; set; }
        public int womensCount { get; set; }

        private Settings _settings;

        string score = string.Empty;
        int courseCount = 0;
        //int classCount = 0;
        string time = string.Empty;
        string imageSize = "15";
        char hyphen = '-';

        HtmlDocument doc = new HtmlDocument();
        StringBuilder html = new StringBuilder();

        public DisplayResults(Settings settings)
        {
            _settings = settings;
            BaseHTMLFile();
        }

        private void BaseHTMLFile()
        {
            html.Append("<html>");
            html.Append("<head>");
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
            html.Append(Program.competition[0].Name);
            html.Append("</h2>");
            html.Append("</div>");
            html.Append("<div class='col-sm'>");
            html.Append("<h3>");
            html.Append("Number of competitors: ");
            html.Append(Program.CompetitorCourseSummaries.Count.ToString());
            html.Append("</h3>");
            html.Append("</div>");
            html.Append("</div>");

            html.Append("<div class='row'>");

            DisplayColumns();

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

        private void DisplayColumns()
        {
            //calculate rows per column
            var dataCount = Program.CompetitorCourseSummaries.Count;
            var coursesCount = Program.courses.Count;
            double dataCountDbl = (((coursesCount * 2) + dataCount) / 3);
            int dataToTake = (int)Math.Ceiling(dataCountDbl);
            int dataToSkip = 0;

            var course = string.Empty;

            //Column 1
            var currentCourse = BulldHTMLTable(Program.CompetitorCourseSummaries.ToList(), dataToSkip, dataToTake, null);
            dataToSkip = dataToSkip + dataToTake;

            //Column 2
            currentCourse = BulldHTMLTable(Program.CompetitorCourseSummaries, dataToSkip, dataToTake, currentCourse);
            dataToSkip = dataToSkip + dataToTake;

            //column 3
            currentCourse = BulldHTMLTable(Program.CompetitorCourseSummaries, dataToSkip, dataCount - dataToSkip, currentCourse);
        }

        private HTMLHelper BulldHTMLTable(List<CompetitorResultSummary> dataList, int dataToSkip, int dataToTake, HTMLHelper htmlHelper)
        {
            var data1 = dataList.Skip(dataToSkip).Take(dataToTake).ToList();
            var data = data1.GroupBy(c => c.CourseId)
                .Select(group => new { course = group.Key, Items = group.ToList() })
                .ToList();

            var prevCourse = string.Empty;
            var prevClass = string.Empty;
            html.Append("<div class='col-lg'>");

            foreach (var course in data)
            {
                var courseDetails = Shared.GetCourseDetails(course.course);
                html.Append("<table class='table'>");
                html.Append("<thead>");
                html.Append("<tr class='class-header'>");
                html.Append("<th class='course-header' scope='col'>");

                if (htmlHelper == null)
                    html.Append(course.course);
                else
                    if (htmlHelper.Course == course.course)
                        html.Append(htmlHelper.Course + "...");
                else
                    html.Append(course.course);
                html.Append("</th>");

                if (course.course != Constants.COURSE_TYPE_SCORE)
                    html.Append("<th scope='col'>Class #</th>");
                html.Append("<th scope='col'>Status</th>");
                html.Append("<th scope='col'>Name</th>");
                html.Append("<th scope='col'>Time</th>");

                if (course.course == Constants.COURSE_TYPE_SCORE)
                    html.Append("<th scope='col'>Score</th>");
                else
                    html.Append("<th scope='col'></th>");

                html.Append("</tr>");
                html.Append("</thead>");
                html.Append("<body>");

                for (int i = 1; i <= course.Items.Count; i++)
                {
                    if (((htmlHelper == null) && (i == 1)))
                    {
                        courseCount = 0;
                        mensCount = 0;
                        womensCount = 0;
                    }
                    else if ((htmlHelper != null) && (htmlHelper.IsNewTable == true))
                    {
                        courseCount = htmlHelper.CourseCount;
                        mensCount = htmlHelper.ClassCountMen;
                        womensCount = htmlHelper.ClassCountWomen;
                        htmlHelper.IsNewTable = false;
                    } else if ((htmlHelper != null) && (i==1))
                    {
                        courseCount = 0;
                        mensCount = 0;
                        womensCount = 0;
                    }

                    courseCount++;
                    html.Append("<tr");
                    html.Append(" class='");

                    html.Append(Shared.GetGenderFromClass(Program.GetCompetitorClass(course.Items[i - 1])));
                    html.Append("'");
                    html.Append(">");
                    html.Append("<th scope ='row'>");
                    html.Append(courseCount.ToString());
                    html.Append("</th>");

                    if (course.course != Constants.COURSE_TYPE_SCORE)
                    {
                        html.Append("<td class='class-count'>");
                        if (Shared.GetGenderFromClass(Program.GetCompetitorClass(course.Items[i - 1])) == Constants.CLASS_TYPE_MEN) 
                        {
                            if (course.Items[i-1].Status != (int)Status.DidNotStart)
                            {
                                mensCount++;
                                html.Append(mensCount.ToString());
                            }
                        }
                        else
                        {
                            if (course.Items[i - 1].Status != (int)Status.DidNotStart)
                            {
                                womensCount++;
                                html.Append(womensCount.ToString());
                            }
                        }
                        html.Append("</td>");
                    }

                    html.Append("<td class='class-count'>");
                    html.Append("<img src='Images/");
                    html.Append(Shared.GetEnumValue(course.Items[i - 1].Status));
                    html.Append(hyphen + imageSize);
                    html.Append(".png' class='img-responsive'>");
                    html.Append("</td>");
                    html.Append("<td>");

                    html.Append(Program.GetName(course.Items[i - 1].SI));
                    html.Append("</td>");

                    var elapsedTime = course.Items[i - 1].ElapsedTime;
                    html.Append("<td class='elapsed-time'>");

                    if ((elapsedTime == TimeSpan.MaxValue) ||
                       (elapsedTime == TimeSpan.Zero))
                        html.Append(string.Empty);
                    else
                        html.Append(elapsedTime.ToString());

                    html.Append("</td>");
                    html.Append("<td>");
                    html.Append(score = (course.Items[i - 1].Score == 0) ? string.Empty : course.Items[i - 1].Score.ToString());
                    html.Append("</td>");
                    html.Append("</tr>");
                    prevCourse = course.course;
                    prevClass = Program.GetCompetitorClass(course.Items[i - 1]);
                }
                html.Append("</table>");
            }
           
            html.Append("</div>");

            return new HTMLHelper {
                Course = prevCourse,
                CourseCount = courseCount,
                ClassCountMen = mensCount,
                ClassCountWomen = womensCount,
                IsNewTable = true
            };
        }

    }
}
