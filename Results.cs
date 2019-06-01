using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_Results
{
    public class Results
    {
        private List<CompetitorControl> _coursePunches;

        public Results(List<CompetitorControl> coursePunches)
        {
            _coursePunches = coursePunches;
        }

        private void Initialise()
        {
            GenerateResults();
            //SortResults();
            //DisplayResults();
        }

        private void GenerateResults()
        {
            foreach (var competitorPunches in _coursePunches)
            {
                var competitorCourseSummary = new CompetitorResultSummary();
                competitorCourseSummary.SI = competitorPunches.SI;
                competitorCourseSummary.ClassId = competitors.FirstOrDefault(s => s.SI == competitorPunches.SI).ClassId;

                competitorCourseSummary.CourseId =
                    (GetCompetitorCourse(competitorCourseSummary) == null)
                    ? string.Empty
                    : GetCompetitorCourse(competitorCourseSummary);

                competitorCourseSummary.StartTime = competitorPunches.CompetitorControls[0].CoursePunchTime;
                if (competitorCourseSummary.StartTime == TimeSpan.Zero)
                {
                    competitorCourseSummary.Status = (int)Status.DidNotStart;
                }
                else
                {
                    if (competitorPunches.CompetitorControls.Any(s => s.CoursePunchName == "F"))
                    {
                        var finishTime = competitorPunches.CompetitorControls.SingleOrDefault(s => s.CoursePunchName == "F").CoursePunchTime;
                        if (finishTime == TimeSpan.Zero)
                        {
                            competitorCourseSummary.Status = (int)Status.DidNotFinish;
                        }
                        else
                        {
                            competitorCourseSummary.FinishTime = competitorPunches.CompetitorControls.SingleOrDefault(s => s.CoursePunchName == "F").CoursePunchTime;
                            CalculateElapsedTime(competitorCourseSummary);
                            if (competitorCourseSummary.ElapsedTime < TimeSpan.Zero)
                            {
                                competitorCourseSummary.Status = (int)Status.DidNotFinish;
                                competitorCourseSummary.ElapsedTime = TimeSpan.Zero;

                            }
                            else competitorCourseSummary.Status = (int)Status.Finished;

                            if (IsScoreCourse(competitorCourseSummary.CourseId))
                            {
                                CalculateScore(competitorPunches.CompetitorControls, competitorCourseSummary);

                            }
                            else
                            {
                                CheckValidLineCourse(competitorPunches.CompetitorControls, competitorCourseSummary);
                            }

                        }
                    }
                    else
                    {
                        GetStatus(competitorCourseSummary);
                    }
                }
                CompetitorCourseSummaries.Add(competitorCourseSummary);
            }
        }


    }
}
