using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_Results.Services
{
    public class CourseService
    {
        public List<Course> courses { get; set; }

        public List<Course> GetAll()
        {
            return courses;
        }

        public Course Get(string id)
        {
            return courses.FirstOrDefault(c => c.CourseId == id);
        }

        public bool CompetitorCourseExists(string courseId)
        {
            return courses.Any(c => c.CourseId == courseId);
        }

        public Course GetCourseBySI(int SI, CompetitorService competitorService)
        {
            //var courseId = competitors.FirstOrDefault(c => c.SI == SI).CourseId;
            var courseId = competitorService.Get(SI).CourseId;
            if (courseId == null) { return null; }

            return courses.FirstOrDefault(c => c.CourseId == courseId);
        }

        public bool IsScoreCourse(string courseId)
        {
            if (courses.First(c => c.CourseId == courseId).CourseType == Constants.COURSE_TYPE_SCORE)
                return true;

            return false;
        }

        //public Course GetCourseDetails(string courseId)
        //{
        //    return courses.FirstOrDefault(c => c.CourseId == courseId);
        //}
    }
}
