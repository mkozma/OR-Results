using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_Results
{
    public class DisplayResults
    {
        public int mensCount { get; set; }
        public int womensCount { get; set; }

        string score = string.Empty;
        int courseCount = 0;
        int classCount = 0;
        string time = string.Empty;
        string imageSize = "25";
        char hyphen = '-';

        HtmlDocument doc = new HtmlDocument();
        StringBuilder html = new StringBuilder();


        public DisplayResults()
        {
            

            html.Append("<html>");
            html.Append("<head>");
            html.Append("<Link rel='stylesheet' href='css/bootstrap.min.css'>");
            html.Append("<Link rel='stylesheet' href='css/main.css'>");

            html.Append("</head>");
            html.Append("<body>");

            html.Append("<div class='container-fluid'>");

            html.Append("<div class='row'>");
            html.Append("<h1>");
            html.Append(Program.competition[0].Name);
            html.Append("</h1>");
            html.Append("</div>");

            html.Append("<div class='row'>");
            html.Append("<div class='col-lg'>");

            //calculate rows per column
            var dataCount = Program.CompetitorCourseSummaries.Count;
            var coursesCount = Program.courses.Count;
            double dataCountDbl = (((coursesCount * 2) + dataCount) / 2);
            int dataToSkip = 0;
            int dataToTake = (int)Math.Ceiling(dataCountDbl);

            var course = string.Empty;

            //Column 1
            var currentCourse = BulldHTMLTable(Program.CompetitorCourseSummaries.ToList(),dataToSkip, dataToTake, course);

            //get the current course

            //Column 2
            currentCourse = BulldHTMLTable(Program.CompetitorCourseSummaries, dataToTake, dataCount-dataToTake, currentCourse);

            html.Append("</div>");
            html.Append("</div>");
            html.Append("</div>");
            html.Append("</div>");

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

        private string BulldHTMLTable(List<CompetitorResultSummary> dataList, int dataToSkip, int dataToTake, string currentCourse)
        {
            var data1 = dataList.Skip(dataToSkip).Take(dataToTake).ToList();
            var data = data1.GroupBy(c => c.ClassId)
                .Select(group => new { course = group.Key, Items = group.ToList() })
                .ToList();

            var prevCourse = string.Empty;
            var prevClass = string.Empty;

            foreach (var course in data)
            {
                var courseDetails = Shared.GetCourseDetails(course.course);

                html.Append("<table class='table'>");
                html.Append("<thead>");
                html.Append("<tr>");
                html.Append("<h3>");
                //check if the course contines from previous column
                if (currentCourse == string.Empty)
                    html.Append(course.course);
                else
                    if (currentCourse == course.course)
                        html.Append(currentCourse + " (cont.)");
                else
                    html.Append(course.course);
                html.Append("</h3>");
                html.Append("</tr>");

                html.Append("<tr class='class-header'>");
                html.Append("<th scope='col'>#</th>");
                html.Append("<th scope='col'>Class #</th>");
                //html.Append("<th scope='col'>Status</th>");
                html.Append("<th scope='col'>Status</th>");
                html.Append("<th scope='col'>SI</th>");
                html.Append("<th scope='col'>Name</th>");
                html.Append("<th scope='col'>Time</th>");
                if (course.course == "Score")
                    html.Append("<th scope='col'>Score</th>");
                else
                    html.Append("<th scope='col'></th>");
                html.Append("</tr>");

                html.Append("</thead>");

                html.Append("<body>");


                for (int i = 1; i <= course.Items.Count; i++)
                {
                    courseCount = (prevCourse == course.course) ? ++courseCount : 1;
                    //classCount = (prevClass == Program.GetCompetitorClass( course.Items[i-1])) ? ++classCount : 1;
                    if (courseCount == 1)
                    {
                        mensCount = 0;

                        womensCount = 0;
                    }

                    html.Append("<tr");
                    html.Append(" class='");

                    html.Append(Shared.GetGenderFromClass(Program.GetCompetitorClass(course.Items[i - 1])));
                    html.Append("'");
                    html.Append(">");
                    html.Append("<th scope ='row'>");
                    html.Append(courseCount.ToString());
                    html.Append("</th>");
                    html.Append("<td class='class-count'>");
                    //html.Append(Program.GetCompetitorClass(course.Items[i - 1]));
                    if (Shared.GetGenderFromClass(Program.GetCompetitorClass(course.Items[i - 1])) == "men")
                    {
                        mensCount++;
                        html.Append(mensCount.ToString());
                    }
                    else
                    {
                        womensCount++;
                        html.Append(womensCount.ToString());
                    }

                    //html.Append(classCount.ToString());
                    html.Append("</td>");
                    //html.Append("<td>");
                    //html.Append(Shared.GetEnumValue(course.Items[i-1].Status));
                    //html.Append("</td>");
                    html.Append("<td class='class-count'>");
                    html.Append("<img src='Images/");
                    html.Append(Shared.GetEnumValue(course.Items[i - 1].Status));
                    html.Append(hyphen + imageSize);
                    html.Append(".png' class='img-responsive'>");
                    html.Append("</td>");
                    html.Append("<td>");
                    html.Append(course.Items[i - 1].SI);
                    html.Append("</td>");
                    html.Append("<td>");
                    html.Append(Program.GetName(course.Items[i - 1].SI));
                    html.Append("</td>");
                    html.Append("<td>");
                    html.Append(course.Items[i - 1].ElapsedTime);
                    html.Append("</td>");
                    html.Append("<td>");
                    html.Append(score = (course.Items[i - 1].Score == 0) ? string.Empty : course.Items[i - 1].Score.ToString());
                    html.Append("</td>");
                    html.Append("</tr>");
                    prevCourse = course.course;
                    prevClass = Program.GetCompetitorClass(course.Items[i - 1]);
                    //if (prevClass == Program.GetCompetitorClass(course.Items[i - 1]))
                    //    mensCount++;
                    //else
                    //    womensCount++;
                }
                html.Append("</table>");
            }

            html.Append("</div>");
            html.Append("<div class='col-lg'>");
            html.Append("<div class='row'>");

            return prevCourse;

        }

    }
}
