using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_Results.Services
{
    public class CompetitorService
    {
        public List<Competitor> competitors { get; set; }

        public List<Competitor> GetAll()
        {
            return competitors;
        }

        public Competitor Get(int id)
        {
            return competitors.FirstOrDefault(c => c.SI == id);
        }

        public string GetCompetitorCourse(CompetitorResultSummary competitorCourseSummary)
        {
            return competitors.FirstOrDefault(c => c.SI == competitorCourseSummary.SI).CourseId;
        }

        public string GetCompetitorClass(CompetitorResultSummary competitorResultSummary)
        {
            return competitors.FirstOrDefault(c => c.SI == competitorResultSummary.SI).ClassId;
        }

        public string GetCompetitorClass(int SI)
        {
            return competitors.FirstOrDefault(c => c.SI == SI).ClassId;
        }

        public string GetCourseIdBySI(int SI)
        {
            return competitors.FirstOrDefault(c => c.SI == SI).CourseId;
        }

        public bool isValidSI(int sI)
        {
            return competitors.Any(c => c.SI == sI);
        }
    }
}
