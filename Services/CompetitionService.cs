using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_Results.Services
{
    public class CompetitionService
    {
        public List<Competition> competition;

        public List<Competition> GetAll()
        {
            return competition;
        }

        public Competition GetFirst()
        {
            return competition.FirstOrDefault();
        }
    }
}
