using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_Results.Services
{
    public class CourseVariantService
    {
        public List<CourseVariant> courseVariants { get; set; }

        public CourseVariant Get(string id)
        {
            return courseVariants.FirstOrDefault(c => c.CourseId == id);
        }
    }
}
