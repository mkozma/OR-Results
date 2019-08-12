using System;
using System.Collections.Generic;
using System.Linq;

namespace OR_Results
{
    public class DisplayResults
    {
        private Settings _settings;
        private string score = string.Empty;
        private string netscore = string.Empty;
        private int courseCount = 0;
        private string time = string.Empty;
        private const int NUMBER_OF_COLUMNS = 3;
        private TableRowCount tableRowCount = null;
        private List<TableRowCount> listTableRowCount = new List<TableRowCount>();

        public int mensCount { get; set; }
        public int womensCount { get; set; }
        public List<DisplayRow> listDisplayRow = new List<DisplayRow>();
        public List<DisplayCourse> listDisplayCourses = new List<DisplayCourse>();
        public List<DisplayTable> listDisplayTable = new List<DisplayTable>();
        public int DataToSkip { get; set; }
        public int DataToTake { get; set; }

        public int DataCount
        {
            get { return Program.CompetitorCourseSummaries.Count; }
        }

        public DisplayResults(Settings settings)
        {
            _settings = settings;

            CalculateDataPerColumn();

            CalculateDataInTables();

            BuildDisplayTable();

        }

        private void CalculateDataPerColumn()
        {
            var coursesCount = Program.courses.Count;
            double dataCountDbl = (((coursesCount * 2) + DataCount) / NUMBER_OF_COLUMNS);
            DataToTake = (int)Math.Ceiling(dataCountDbl);
            DataToSkip = 0;
        }

        private void CalculateDataInTables()
        {
            listTableRowCount = new List<TableRowCount>();
            var remainingData = DataCount;
            for (int i = 1; i<= NUMBER_OF_COLUMNS; i++)
            {
                tableRowCount = new TableRowCount();
                tableRowCount.Id = i;
                tableRowCount.DataToSkip = DataToSkip;

                //test for get remaining 
                DataToTake = (DataToTake < remainingData) ? DataToTake : remainingData;
                tableRowCount.DataToTake = DataToTake;
                remainingData -= DataToTake;

                listTableRowCount.Add(tableRowCount);
                DataToSkip += DataToTake;
            }

        }

        private void BuildDisplayTable()
        {
            TableRowCount lastTableRowCount = null;

            foreach (TableRowCount item in listTableRowCount)
            {
                if (lastTableRowCount != null)
                {
                    item.LastCourseCount = lastTableRowCount.LastCourseCount;
                    item.LastCourse = lastTableRowCount.LastCourse;
                    item.LastMensClassCount = lastTableRowCount.LastMensClassCount;
                    item.LastWomensClassCount = lastTableRowCount.LastWomensClassCount;
                }

                var result = BuildDisplayRow(Program.CompetitorCourseSummaries.Skip(item.DataToSkip).Take(item.DataToTake).ToList(), item);

                lastTableRowCount = item;
            }
        }

        private TableRowCount BuildDisplayRow(List<CompetitorResultSummary> competitorResultSummaries, TableRowCount tableRowCount)
        {
            var data = competitorResultSummaries.GroupBy(c => c.CourseId)
                .Select(group => new { course = group.Key, Items = group.ToList() })
                .ToList();

            int mensClassCount;
            int womensClassCount;

            var isContunation = false;

            if (tableRowCount.LastCourse == data[0].course)
                isContunation = true;

            courseCount = 1;
            mensClassCount = 1;
            womensClassCount = 1;

            for (int i=1; i<= data.Count; i++)
            {
                if (isContunation)
                {
                    courseCount =  tableRowCount.LastCourseCount;
                    mensClassCount = tableRowCount.LastMensClassCount;
                    womensClassCount = tableRowCount.LastWomensClassCount;
                    isContunation = false;
                }
                else
                {
                    courseCount = 0;
                    mensClassCount = 0;
                    womensClassCount = 0;
                }

                for (int j = 1; j <= data[i - 1].Items.Count; j++)
                {
                    var leaderTime = data[i-1].Items[0].ElapsedTime;

                    var item = data[i - 1].Items[j - 1];

                    courseCount += 1;
                    if (Shared.GetGenderFromClass(item.ClassId) == "men")
                        mensClassCount += 1;
                    else
                        womensClassCount += 1;

                    var genderCount = (Shared.GetGenderFromClass(item.ClassId) == "men") ? (mensClassCount).ToString() : (womensClassCount).ToString();

                    var displayRow = new DisplayRow
                    {
                        Id = courseCount,
                        Course = data[i-1].course,
                        Class = item.ClassId,
                        ClassCount = genderCount,
                        Status = item.Status,
                        Name = Shared.GetName(item.SI),
                        SI = item.SI.ToString(),
                        ElapsedTime =  (TimeSpan)item.ElapsedTime,
                        TimeDifference = Shared.GetTimeDiffFromLeader(leaderTime, item.ElapsedTime, item.Status, j),
                        Penalty = item.Penalty,
                        NetScore = (item.Score - item.Penalty)
                    };

                    listDisplayRow.Add(displayRow);
                }

                var displayCourse = new DisplayCourse
                {
                    Id = i,
                    CourseId = data[i - 1].course,
                    LeaderTime = (i == 1) ? data[i - 1].Items[0].ElapsedTime : null,
                    ListDisplayRows = listDisplayRow
                };

                listDisplayCourses.Add(displayCourse);

                tableRowCount.LastCourseCount = listDisplayRow.Select(c => c.Id).Max();
                tableRowCount.LastMensClassCount = mensClassCount;
                tableRowCount.LastWomensClassCount = womensClassCount;
                tableRowCount.LastCourse = listDisplayRow.Last().Course;

                listDisplayRow = new List<DisplayRow>();
            }
           
            //Build the table
            AddToDisplayTable(listDisplayCourses, tableRowCount.Id);
            listDisplayRow = new List<DisplayRow>();
            listDisplayCourses = new List<DisplayCourse>();
            return tableRowCount;
        }

        private void AddToDisplayTable(List<DisplayCourse> listDisplayCourses, int tableIndex)
        {
            var displayTable = new DisplayTable
            {
                Id = tableIndex,
                ListDisplayCourses = listDisplayCourses
            };

            listDisplayTable.Add(displayTable);
        }

        #region Redundant


        //private void DisplayColumns()
        //{
        //    //calculate rows per column
        //    var dataCount = Program.CompetitorCourseSummaries.Count;
        //    var coursesCount = Program.courses.Count;
        //    double dataCountDbl = (((coursesCount * 2) + dataCount) / 3);
        //    int dataToTake = (int)Math.Ceiling(dataCountDbl);
        //    int dataToSkip = 0;

        //    var course = string.Empty;

        //    //Column 1
        //    //var currentCourse = BulldHTMLTable(Program.CompetitorCourseSummaries.ToList(), dataToSkip, dataToTake, null);
        //    dataToSkip = dataToSkip + dataToTake;



        //    //Column 2
        //    //currentCourse = BulldHTMLTable(Program.CompetitorCourseSummaries, dataToSkip, dataToTake, currentCourse);
        //    dataToSkip = dataToSkip + dataToTake;


        //    //column 3
        //    //currentCourse = BulldHTMLTable(Program.CompetitorCourseSummaries, dataToSkip, dataCount - dataToSkip, currentCourse);
        //}

        #endregion
    }
}
