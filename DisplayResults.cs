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
        public DisplayResults()
        {
            var doc = new HtmlDocument();
            var html = new StringBuilder();

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
            html.Append("<h3>Column 1</h3>");



            var score = string.Empty;
            var courseCount = 0;
            var classCount = 0;
            var time = string.Empty;


            //calculate rows per column
            var dataCount = Program.CompetitorCourseSummaries.Count;
            var coursesCount = Program.courses.Count;
            double dataCountDbl = (((coursesCount * 2) + dataCount) / 2);
            int dataPerCol = (int)Math.Ceiling(dataCountDbl);

            var data1 = Program.CompetitorCourseSummaries.Take(dataPerCol).ToList();
            var data2 = Program.CompetitorCourseSummaries.Skip(dataCount - dataPerCol).ToList();

            var data = data1.GroupBy(c => c.ClassId)
                 .Select(group => new { course = group.Key, Items = group.ToList() })
                 .ToList();

            var data3 = data2.GroupBy(c => c.ClassId)
                 .Select(group => new { course = group.Key, Items = group.ToList() })
                 .ToList();


            foreach (var course in data)
            
            //foreach (var course in data)
            {
                var courseDetails = Shared.GetCourseDetails(course.course);

                html.Append("<table class='table'>");
                html.Append("<thead>");
                html.Append("<tr>");
                html.Append("<h3>");
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
                var prevCourse = string.Empty;
                var prevClass = string.Empty;

                for (int i = 1; i <= course.Items.Count; i++)
                {
                    courseCount = (prevCourse == course.course) ? ++courseCount : 1;
                    classCount = (prevClass == Program.GetCompetitorClass( course.Items[i-1])) ? ++classCount : 1;
                    html.Append("<tr");
                    html.Append(" class='");
                    html.Append(Shared.GetGenderFromClass(Program.GetCompetitorClass(course.Items[i-1])));
                    html.Append("'");
                    html.Append(">");
                    html.Append("<th scope ='row'>");
                    html.Append(courseCount.ToString());
                    html.Append("</th>");
                    html.Append("<td>");
                    //html.Append(Program.GetCompetitorClass(course.Items[i - 1]));
                    html.Append(classCount.ToString());
                    html.Append("</td>");
                    //html.Append("<td>");
                    //html.Append(Shared.GetEnumValue(course.Items[i-1].Status));
                    //html.Append("</td>");
                    html.Append("<td>");
                    html.Append("<img src='Images/");
                    html.Append(Shared.GetEnumValue(course.Items[i - 1].Status));
                    html.Append(".png' class='img-responsive'>");
                    //html.Append(Shared.GetEnumValue(course.Items[i-1].Status));
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
                }
                html.Append("</table>");
            }

            html.Append("</div>");
            html.Append("<div class='col-lg'>");
            html.Append("<div class='row'>");
            html.Append("<h3>Column 2</h3>");


            foreach (var course in data3)

            //foreach (var course in data)
            {
                var courseDetails = Shared.GetCourseDetails(course.course);

                html.Append("<table class='table'>");
                html.Append("<thead>");
                html.Append("<tr>");
                html.Append("<h3>");
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
                var prevCourse = string.Empty;
                var prevClass = string.Empty;

                for (int i = 1; i <= course.Items.Count; i++)
                {
                    courseCount = (prevCourse == course.course) ? ++courseCount : 1;
                    classCount = (prevClass == Program.GetCompetitorClass(course.Items[i - 1])) ? ++classCount : 1;
                    html.Append("<tr");
                    html.Append(" class='");
                    html.Append(Shared.GetGenderFromClass(Program.GetCompetitorClass(course.Items[i - 1])));
                    html.Append("'");
                    html.Append(">");
                    html.Append("<th scope ='row'>");
                    html.Append(courseCount.ToString());
                    html.Append("</th>");
                    html.Append("<td>");
                    //html.Append(Program.GetCompetitorClass(course.Items[i - 1]));
                    html.Append(classCount.ToString());
                    html.Append("</td>");
                    //html.Append("<td>");
                    //html.Append(Shared.GetEnumValue(course.Items[i-1].Status));
                    //html.Append("</td>");
                    html.Append("<td>");
                    html.Append("<img src='Images/");
                    html.Append(Shared.GetEnumValue(course.Items[i - 1].Status));
                    html.Append(".png' class='img-responsive'>");
                    //html.Append(Shared.GetEnumValue(course.Items[i-1].Status));
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
                }
                html.Append("</table>");
            }
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
    }
}
