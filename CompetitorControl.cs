using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_Results
{
    public class CompetitorControl
    {
        public string CoursePunchName { get; set; }
        public TimeSpan CoursePunchTime { get; set; }
    }

    public class CompetitorControlComparer : IEqualityComparer<CompetitorControl>
    {
        public bool Equals(CompetitorControl x, CompetitorControl y)
        {
            return x.CoursePunchName == y.CoursePunchName;
        }

        public int GetHashCode(CompetitorControl obj)
        {
            return obj.CoursePunchName.GetHashCode();
        }
    }

}
