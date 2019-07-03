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
            html.Append("<Link rel='stylesheet' href='/css/bootstrap.min.css'>");
            html.Append("<Link rel='stylesheet' href='/css/bootstrap-theme.min.css'>");

            html.Append("</head>");
            html.Append("<body>");
            html.Append("<h1>OR Results</h1>");
           
            html.Append("</body>");
            html.Append("<table class='table'>");
            html.Append("<thead>");

            html.Append("<tr>");
            html.Append("<th scope='col'>#</th>");
            html.Append("<th scope='col'>course #</th>");
            html.Append("<th scope='col'>class #</th>");
            html.Append("<th scope='col'>status</th>");
            html.Append("<th scope='col'>SI</th>");
            html.Append("<th scope='col'>Name</th>");
            html.Append("<th scope='col'>Time</th>");

            html.Append("</tr>");
            html.Append("</thead>");
            html.Append("<body>");

            //foreach (var competitorSummary in Program.competitorCourseSummaries)
            //{
            var score = string.Empty;
            var courseCount = 0;
            var data = Program.CompetitorCourseSummaries;
            var time = string.Empty;

            var prevCourse = string.Empty;
            for (int i = 1; i <= data.Count; i++)
            {
                courseCount = (prevCourse == data[i - 1].CourseId) ? ++courseCount : 1;
                html.Append("<tr>");
                html.Append("<th scope='row'>");
                html.Append(i.ToString());
                html.Append("</th>");
                html.Append("<td>");
                html.Append(courseCount.ToString());
                html.Append("</td>");
                html.Append("<td>");
                html.Append(data[i-1].CourseId);
                html.Append("</td>");
                html.Append("<td>");
                html.Append(Shared.GetEnumValue(data[i - 1].Status));
                html.Append("</td>");
                html.Append("<td>");
                html.Append(data[i - 1].SI);
                html.Append("</td>");
                html.Append("<td>");
                html.Append(Program.GetName(data[i - 1].SI));
                html.Append("</td>");
                html.Append("<td>");
                html.Append(data[i - 1].ElapsedTime);
                html.Append("</td>");
                html.Append("</tr>");
                prevCourse = data[i - 1].CourseId;
            }
            html.Append("<script src='/js/jquery.min.js'></script>");
            html.Append("<script src='/js/bootstrap.min.js'></script>");
            html.Append("</body>");

            html.Append("</table>");

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
