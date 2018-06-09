using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureExtraction
{
    class FeatureGenerator
    {
        public string studentId { set; get; }
        public string studentResult { set; get; }
        public double TotalTimeOnExam { set; get; } // change type from timeSpan
        public int NumberOfMovementsThroughExam { set; get; }
        public double OutOfTimePercentage { set; get; }
        public double ResultOfLastFiveAnswers { set; get; }
        public double AvgTimeSpanOnItemToGetTheFinalAnswer { set; get; }// change type from timeSpan
        public double AvgTimeSpentOnItemForAllVisits { set; get; }// change type from timeSpan
        public double AvgTimeSpanOnReviewingItem { set; get; }// change type from timeSpan
        public double AvgTimeSpentOnReviewingItemAfterFinalAnswer { set; get; }// change type from timeSpan
        public int NumberOfQuestionGuessed { set; get; }
        public int NumberOfQuestionGuessedBasedOnCorrectAnswers { set; get; }
        public int NumberOfQuestionUncertain { set; get; }
        public int NumberOfQuestionUncertainBasedOnCorrectAnswers { set; get; }
        public int NumberOfItemsAnswersChangedFirstLast { set; get; }
        public int NumberOfItemsAnswersChanged { set; get; }
        public int NumberOfQuestionsAnswerChangedButReturnToFirstOne { set; get; }
        public double PercentageOfQuestionsAswered { set; get; }
        public double AvgNumberOfVesitsBeforeFinalAnswer { set; get; }
        public double AvgNumberOfVisitsAfterFirstAnswer { set; get; }
        public double AvgVisitsToItemAllTimes { set; get; }
        public double AvgNumberOfVisitsWithNoAnswer { set; get; }
        public double TotalNumberOfVisitsToAllQuestions { set; get; }
        public double AvgTimeOnCorrectAnswers { set; get; }// change type from timeSpan
        public double AvgTimeOnWrongAnswers { set; get; }// change type from timeSpan
        public int NumberOfItemsAnswerWasFromRightToWrong { set; get; }
        public int NumberOfItemsAnswerWasFromWrongToRight { set; get; }
        public int NumberOfItemsAnswerWasFromWrongToWrong { set; get; }
        public int NumberOfItemsAnswerWasFromRightToWrongFirstAndLast { set; get; }
        public int NumberOfItemsAnswerWasFromWrongToRightFirstAndLast { set; get; }
        public int NumberOfItemsAnswerWasFromWrongToWrongFirstAndLast { set; get; }
        public int NumberOfItemsFromRightToWrongToRight { set; get; }
        public int NumberOfSkipsThroughTheTest { set; get; }
        private List<string> triggerToEliminateOne = new List<string>() { "10 minute save", "exam closed", "final", "final2", "exam start", "exam close", "exambegin" };
        private List<string> triggerToEliminateTwo = new List<string>() { "final", "final2" };
        internal Dictionary<string, TimeSpan> TimeSpanOnEachItem { set; get; }  //Key: Question, Value: Timespan
        internal Dictionary<string, TimeSpan> TimeSpanOnCorrectAnswerForItem { set; get; }
        private Dictionary<string, string> correctAnswerList = Program.GetCorrectAnswerList();
        public int NumberOfWrongAnswerGuessed = 0;

        public string PrepareForCsvFile()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($"{this.studentId}, " + 
                            $"{this.studentResult}, " + 
                            $"{this.TotalTimeOnExam}, " + 
                            $"{this.NumberOfMovementsThroughExam}, " +
                            $"{this.OutOfTimePercentage}, " +
                            $"{this.ResultOfLastFiveAnswers}, " +
                            $"{this.AvgTimeSpanOnItemToGetTheFinalAnswer}, " +
                            $"{this.AvgTimeSpanOnReviewingItem}, " + 
                            $"{this.AvgTimeSpentOnReviewingItemAfterFinalAnswer}, " +
                            $"{this.AvgTimeSpentOnItemForAllVisits}, " + 
                            $"{this.NumberOfQuestionGuessed}, " +
                            $"{this.NumberOfQuestionGuessedBasedOnCorrectAnswers}, " +
                            $"{this.NumberOfQuestionUncertain}, " +
                            $"{this.NumberOfQuestionUncertainBasedOnCorrectAnswers}, " +
                            $"{this.NumberOfItemsAnswersChangedFirstLast}, " +
                            $"{this.NumberOfItemsAnswersChanged}, " +
                            $"{this.NumberOfQuestionsAnswerChangedButReturnToFirstOne}, " +
                            $"{this.PercentageOfQuestionsAswered}, " +
                            $"{this.AvgNumberOfVesitsBeforeFinalAnswer}, " + 
                            $"{this.AvgNumberOfVisitsAfterFirstAnswer}, " +
                            $"{this.AvgVisitsToItemAllTimes}, " +
                            $"{this.AvgNumberOfVisitsWithNoAnswer}, " +
                            $"{this.TotalNumberOfVisitsToAllQuestions}, " +
                            $"{this.NumberOfSkipsThroughTheTest}, " +
                            $"{this.AvgTimeOnCorrectAnswers}, " +
                            $"{this.AvgTimeOnWrongAnswers}, " +                        
                            $"{this.NumberOfItemsAnswerWasFromWrongToRight}, " +
                            $"{this.NumberOfItemsAnswerWasFromRightToWrong}, " +
                            $"{this.NumberOfItemsAnswerWasFromWrongToWrong}, " +
                            $"{this.NumberOfItemsAnswerWasFromRightToWrongFirstAndLast}, " +
                            $"{this.NumberOfItemsAnswerWasFromWrongToRightFirstAndLast}, " +
                            $"{this.NumberOfItemsAnswerWasFromWrongToWrongFirstAndLast}, " +
                            $"{this.NumberOfWrongAnswerGuessed}, " +
                            $"{this.NumberOfItemsFromRightToWrongToRight}\n");

            return sb.ToString();
        }
        private TimeSpan CalculateTimeSpaneOnItem(List<int> index, List<Snapshot> snapshotTable)
        {
            TimeSpan TimeSpanOnEachSanpshotForItem = new TimeSpan();
            // calculating the time spent in each snapshot recorded before the final answer by subtracting the timestamp of a snapshot with the previous one
            foreach (var id in index) 
            {
                var previoustItem = snapshotTable.SingleOrDefault(x => id - x.Index == 1);
                var currentItem = snapshotTable.SingleOrDefault(x => x.Index == id);

                if (previoustItem == null)
                {
                    previoustItem = currentItem;                        
                }

                // Calculation
                var timeOfCurrentItem = Convert.ToDateTime(currentItem.TimeStamp);
                var timeOfPreviouseItem = Convert.ToDateTime(previoustItem.TimeStamp);
                
                TimeSpan substractResult = timeOfCurrentItem - timeOfPreviouseItem;
                TimeSpanOnEachSanpshotForItem = TimeSpanOnEachSanpshotForItem.Add(substractResult);                
            }
            
            return TimeSpanOnEachSanpshotForItem;
        }
        private double CalculateAvgTime(List<TimeSpan> timeSpanList)
        {
            double DoubleAvgTicks = timeSpanList.Average(x=> x.Ticks);
            long longAvgTicks = Convert.ToInt64(DoubleAvgTicks);

            //return new TimeSpan(longAvgTicks);
            return DoubleAvgTicks;
        }
        /// <summary>
        /// This method returns the number of movements through the exam by a student by counting the number of actions/triggers except the actions listed on the "triggerToEliminate" list
        /// </summary>
        /// <param name="snapshotTable"> List of all snapshots captured by the system for a student in the exam</param>
        public void ClaculateNumberOfMovementsThroughExam(List<Snapshot> snapshotTable)
        {
            int numberOfMovements;

            numberOfMovements = snapshotTable.Count(x => !triggerToEliminateOne.Contains(x.Trigger.ToLower()));
            this.NumberOfMovementsThroughExam = numberOfMovements;
        }
        /// <summary>
        /// General function that prepares the data for time calculation
        /// </summary>
        /// <param name="snapshotTable"></param>
        /// <param name="timeSabanList"></param>
        /// <param name="type"></param>
        private void GeneralFuction(List<Snapshot> snapshotTable, ref List<TimeSpan> timeSabanList, string type)
        {
            var snapshotsGroupedByItem = snapshotTable.GroupBy(x => x.ItemNum);
            //var test = -1;
            //foreach (var w in snapshotsGroupedByItem)
            //{
            //    test++;
            //    if (w.Key == "1")
            //    {
            //        break;
            //    }
            //}
            foreach (var allSnapshotsForOneItem in snapshotsGroupedByItem/*.Skip(test)*/)
            {
                var snapOfLastVisitOfItem = allSnapshotsForOneItem.LastOrDefault();
                var snapOfFirstVisitOfFinalAnswerOfItem = GetSnapshotOfTheFinalAnswer(allSnapshotsForOneItem);                  

                switch (type)
                {
                    case "time spent on Wrong answer":
                        if (snapOfLastVisitOfItem.Response == "Choice(s): " + correctAnswerList[allSnapshotsForOneItem.Key]){
                            continue;
                        }break;
                    case "time spent on correct answer":
                        if (snapOfLastVisitOfItem.Response != "Choice(s): " + correctAnswerList[allSnapshotsForOneItem.Key]){
                            continue;
                        }break;
                }
                var snapshotsOfVisitsForAnswering = allSnapshotsForOneItem.Where(x => x.Index <= snapOfFirstVisitOfFinalAnswerOfItem.Index);
                var indexOfsnapshotInSnapshotTable = snapshotsOfVisitsForAnswering.Select(x=> x.Index).ToList();// Remove snapshots recorded after the final answer
                timeSabanList.Add(CalculateTimeSpaneOnItem(indexOfsnapshotInSnapshotTable, snapshotTable));
                switch (type)
                {
                    case "add the calculated time to the timing list":
                        this.TimeSpanOnEachItem.Add(allSnapshotsForOneItem.Key, timeSabanList[timeSabanList.Count - 1]); // did change in the first param
                        break;
                    case "time spent on correct answer":
                        this.TimeSpanOnCorrectAnswerForItem.Add(allSnapshotsForOneItem.Key, timeSabanList[timeSabanList.Count - 1]);
                        break;
                }
            }
        }
        /// <summary>
        /// This method returns the average time a student spent on a question. Basicly, it calculates the time on each visit to a question recorded by the system from the first visit of the final answer until the last visit to the question
        /// </summary>
        /// <param name="snapshotTable">List of all snapshots captured by the system for a student in the exam</param>
        public void CalcAvgTimeOnItemToGetTheFinalAnswer(List<Snapshot> snapshotTable)
        {
            var timeSpanOnEachItemList = new List<TimeSpan>();
            this.TimeSpanOnEachItem = new Dictionary<string, TimeSpan>();

            GeneralFuction(snapshotTable, ref timeSpanOnEachItemList, "add the calculated time to the timing list");
            this.AvgTimeSpanOnItemToGetTheFinalAnswer = CalculateAvgTime(timeSpanOnEachItemList);
        }
        /// <summary>
        /// This function returns the average time a student spent on an item for all visits
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void CalcAvgTimeSpentOnItemForAllVisits(List<Snapshot> snapshotTable)
        {
            var snapshotsGroupedByItem = snapshotTable.GroupBy(x => x.ItemNum);
            var timeSpanOnEachItem = new List<TimeSpan>();

            foreach (var allSnapshotsForOneItem in snapshotsGroupedByItem)
            {
                var index = allSnapshotsForOneItem.Where(x => !triggerToEliminateTwo.Contains(x.Trigger.ToLower())).Select(x => x.Index).ToList();
                timeSpanOnEachItem.Add(CalculateTimeSpaneOnItem(index, snapshotTable));
            }
            this.AvgTimeSpentOnItemForAllVisits = CalculateAvgTime(timeSpanOnEachItem);
        }
        /// <summary>
        /// It calculates the average time a student spent in each visit to a qustion after the first visit in which a student chose an answer
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void AvgTimeOnReviewingItem(List<Snapshot> snapshotTable)
        {
            var snapshotsGroupedByItem = snapshotTable.GroupBy(x => x.ItemNum);
            var timeSpanOnEachItem = new List<TimeSpan>();

            foreach (var allSnapshotsForOneItem in snapshotsGroupedByItem)
            {
                var snapOfFirstAnswerOfItem = allSnapshotsForOneItem.FirstOrDefault(x => !string.IsNullOrEmpty(x.Response));

                if (snapOfFirstAnswerOfItem != null)
                {
                    var index = allSnapshotsForOneItem.Where(x => x.Index > snapOfFirstAnswerOfItem.Index).Select(x => x.Index).ToList();
                    timeSpanOnEachItem.Add(CalculateTimeSpaneOnItem(index, snapshotTable));
                }
                else timeSpanOnEachItem.Add(new TimeSpan()); // Unsure about this

            }
            this.AvgTimeSpanOnReviewingItem = CalculateAvgTime(timeSpanOnEachItem);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Snapshot GetSnapshotOfTheFinalAnswer(IGrouping<string, Snapshot> allSnapshotsForOneItem)
        {
            var snapOfFirstAnswerOfItem = allSnapshotsForOneItem.FirstOrDefault();
            var snapOfFirstVisitOfFinalAnswerOfItem = snapOfFirstAnswerOfItem;

            foreach (var snapshot in allSnapshotsForOneItem)
            {
                if (snapshot.Response != snapOfFirstVisitOfFinalAnswerOfItem.Response)
                {
                    snapOfFirstVisitOfFinalAnswerOfItem = snapshot;
                }
            }
            return snapOfFirstVisitOfFinalAnswerOfItem;
        }
        /// <summary>
        /// This function returns the average timespan a student spent on reviewing a question after he made the final answer
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void CalcAvgTimeSpentOnReviewingItemAfterFinalAnswer(List<Snapshot> snapshotTable)
        {
            var snapshotsGroupedByItem = snapshotTable.GroupBy(x => x.ItemNum);
            var timeSpanOnEachItem = new List<TimeSpan>();

            foreach (var allSnapshotsForOneItem in snapshotsGroupedByItem)
            {
                var snapshotOfFinalAnswer = GetSnapshotOfTheFinalAnswer(allSnapshotsForOneItem);
                var indexOfSnapshotsForReviewing = allSnapshotsForOneItem.Where(x => x.Index > snapshotOfFinalAnswer.Index && !triggerToEliminateTwo.Contains(x.Trigger.ToLower())).Select(x=> x.Index).ToList();

                if (!string.IsNullOrEmpty(snapshotOfFinalAnswer.Response))
                {
                    timeSpanOnEachItem.Add(CalculateTimeSpaneOnItem(indexOfSnapshotsForReviewing, snapshotTable));
                }
                else
                    timeSpanOnEachItem.Add(new TimeSpan());
            }
            this.AvgTimeSpentOnReviewingItemAfterFinalAnswer = CalculateAvgTime(timeSpanOnEachItem);
        }
        /// <summary>
        /// It calculates the average number of times a student visited a question before he/she made the final answer
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void CalcAvgNumberOfVisitsBeforeFinalAnswer(List<Snapshot> snapshotTable)
        {
            var snapshotsGroupedByItem = snapshotTable.GroupBy(x => x.ItemNum);
            var numOfVisitsBeforFinalAnswerList = new List<int>();

            foreach (var allSnapshotsForOneItem in snapshotsGroupedByItem)
            {
                var snapshotOfFinalAnswer = GetSnapshotOfTheFinalAnswer(allSnapshotsForOneItem);
                var allSnapshotOfVisitsBeforFinalAnswer = allSnapshotsForOneItem.Where(x => x.Index < snapshotOfFinalAnswer.Index);

                if (allSnapshotOfVisitsBeforFinalAnswer.Count() != 0)
                {
                    var numberOfVisits = allSnapshotOfVisitsBeforFinalAnswer.Where(x => snapshotTable[x.Index + 1].ItemNum != x.ItemNum).Count();
                    numOfVisitsBeforFinalAnswerList.Add(numberOfVisits);
                }
            }
            this.AvgNumberOfVesitsBeforeFinalAnswer = numOfVisitsBeforFinalAnswerList.Average();
        }
        /// <summary>
        /// It calculates the average number of times a student visited a question after he/she made the first answer
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void CalcAvgNumberOfVisitsAfterFirstAnswer(List<Snapshot> snapshotTable)
        {
            var snapshotsGroupedByItem = snapshotTable.GroupBy(x => x.ItemNum);
            var numOfVisitsAfterFinalAnswer = new List<int>();

            foreach (var allSnapshotsForOneItem in snapshotsGroupedByItem)
            {
                var snapshotOfFirstAnswer = allSnapshotsForOneItem.FirstOrDefault(x=> !string.IsNullOrEmpty(x.Response));
                
                if (snapshotOfFirstAnswer != null)
                {
                    var allSnapshotOfVisitsFterFirstAnswer = allSnapshotsForOneItem.Where(x => x.Index > snapshotOfFirstAnswer.Index && !triggerToEliminateTwo.Contains(x.Trigger.ToLower()));
                    var numberOfVisits = allSnapshotOfVisitsFterFirstAnswer.Where(x => snapshotTable[x.Index - 1].ItemNum != x.ItemNum).Count();
                    numOfVisitsAfterFinalAnswer.Add(numberOfVisits);
                }
                else
                    numOfVisitsAfterFinalAnswer.Add(0);
            }
            this.AvgNumberOfVisitsAfterFirstAnswer = numOfVisitsAfterFinalAnswer.Average();
        }
        /// <summary>
        ///  
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void CalcAvgVisitsToItemAllTimes(List<Snapshot> snapshotTable)
        {
            var snapshotsGroupedByItem = snapshotTable.GroupBy(x => x.ItemNum);
            var TotalNumOfVisits = new List<double>();

            foreach (var allSnapshotsForOneItem in snapshotsGroupedByItem)
            {
                var snapshotOfFirstAnswer = allSnapshotsForOneItem.FirstOrDefault();
                var allSnapshotOfVisitsAfterFirstAnswer = allSnapshotsForOneItem.Where(x => x.Index > snapshotOfFirstAnswer.Index && !triggerToEliminateTwo.Contains(x.Trigger.ToLower()));
                var numberOfVisits = 1;

                if (allSnapshotOfVisitsAfterFirstAnswer.Count() != 0)
                {
                    numberOfVisits = allSnapshotOfVisitsAfterFirstAnswer.Where(x => snapshotTable[x.Index - 1].ItemNum != x.ItemNum).Count() +1; // "+1" is for the first visit because the algorithm will not work if the first visit was included in the list and at the biggning of the exam
                }
                TotalNumOfVisits.Add(numberOfVisits);
            }
            this.AvgVisitsToItemAllTimes = TotalNumOfVisits.Average();
        }
        /// <summary>
        /// Average number of times a student visits the same questions and lef them blanck
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void CalcAvgNumberOfVisitsWithNoAnswer(List<Snapshot> snapshotTable)
        {
            var snapshotsGroupedByItem = snapshotTable.GroupBy(x => x.ItemNum);
            var numOfVisitsWithNoAnswer = new List<double>();

            foreach (var allSnapshotsForOneItem in snapshotsGroupedByItem)
            {
                var snapshotOfFirstBlanckAnswer = allSnapshotsForOneItem.FirstOrDefault(x => string.IsNullOrEmpty(x.Response) && snapshotTable[x.Index +1].ItemNum != x.ItemNum); // need to handle an exception taht might rise on "snapshotTable[x.Index +1].ItemNum"

                if (snapshotOfFirstBlanckAnswer != null)
                {
                    var allSnapshotOfVisitsAfterFirstAnswer = allSnapshotsForOneItem.Where(x => x.Index > snapshotOfFirstBlanckAnswer.Index && x.Response == string.Empty &&!triggerToEliminateTwo.Contains(x.Trigger.ToLower()));
                    var numberOfVisits = allSnapshotOfVisitsAfterFirstAnswer.Where(x => snapshotTable[x.Index + 1].ItemNum != x.ItemNum).Count();
                    numOfVisitsWithNoAnswer.Add(numberOfVisits + 1); // Plus one to count the first skip
                }
            }
            if (numOfVisitsWithNoAnswer.Count() != 0)
            {
                this.AvgNumberOfVisitsWithNoAnswer = numOfVisitsWithNoAnswer.Average();
            }
            else
                this.AvgNumberOfVisitsWithNoAnswer = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="snapshotsRefined"></param>
        /// <returns></returns>
        private int GetNumberOfVisits(List<Snapshot> snapshotsRefined)
        {
            var numberOfVisits = default(int);

            foreach (var snapshot in snapshotsRefined)
            {
                if ((snapshotsRefined.Count() -1) != snapshot.Index && snapshot.Index < (snapshotsRefined.Count() - 1))
                {
                    if (snapshot.ItemNum != snapshotsRefined[snapshot.Index +1].ItemNum)
                    {
                        numberOfVisits++;
                    }
                }
                else
                {
                    numberOfVisits++;
                }
            }
            return numberOfVisits;
        }
        /// <summary>
        /// This function counts the number of visits to all questions in the exam by a student
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void CalcTotalNumberOfVisitsToAllQuestions(List<Snapshot> snapshotTable)
        {
            var snapshotsRefined = snapshotTable.Where(x=> !triggerToEliminateTwo.Contains(x.Trigger.ToLower())).ToList();
            var NumOfAllVisits = GetNumberOfVisits(snapshotsRefined);

            this.TotalNumberOfVisitsToAllQuestions = NumOfAllVisits;
        }
        /// <summary>
        /// Counting the total number of skips through the exam by checking the first visit to an item; if there is no answer in the first visit the function reports that as a skip 
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void NumberOfSkips(List<Snapshot> snapshotTable)
        {
            var snapshotsRefined = snapshotTable.Where(x => !triggerToEliminateTwo.Contains(x.Trigger.ToLower()));
            var NumOfSkips = snapshotsRefined.Count(x=> x.Response == string.Empty && snapshotTable[x.Index + 1].ItemNum != x.ItemNum);

            this.NumberOfSkipsThroughTheTest = NumOfSkips;
        }
        /// <summary>
        /// Calculating Z-score
        /// </summary>
        /// <param name="questionWithTimeStudentSpentOn"></param>
        /// <param name="questionListWithStanderdDevAndAvg"></param>
        /// <returns></returns>
        private double ClaculteZScore(KeyValuePair<string, TimeSpan> questionWithTimeStudentSpentOn, Dictionary<string, double[]> questionListWithStanderdDevAndAvg)
        {
            double avgOnQuestion = questionListWithStanderdDevAndAvg[questionWithTimeStudentSpentOn.Key][0];
            double standerdDeviation = Convert.ToInt32(questionListWithStanderdDevAndAvg[questionWithTimeStudentSpentOn.Key][1]);
            double zScore = Math.Abs(Convert.ToDouble(questionWithTimeStudentSpentOn.Value.Ticks) - avgOnQuestion) / standerdDeviation;

            return zScore;
        }
        /// <summary>
        /// If the time a student spent on answering aquetion was lower than the average, it is a guess 
        /// </summary>
        /// <param name="questionListWithStanderdDevAndAvg"></param>
        public void CalcNumberOfItemsGuessed(Dictionary<string, double[]> questionListWithStanderdDevAndAvg)
        {
            int numberOfQuestionGuessed = 0;
            foreach (var question in this.TimeSpanOnEachItem)
            {
                //find z score. If < 1, it's a guess
                var averageTimeOntheQuestion = questionListWithStanderdDevAndAvg[question.Key][0];
                var timeStudentSpentToAnswer = question.Value.Ticks;
                bool isSmallerThanAverage = averageTimeOntheQuestion - timeStudentSpentToAnswer > 0;
                var zScore = ClaculteZScore(question, questionListWithStanderdDevAndAvg);

                if (zScore > 1 && isSmallerThanAverage)
                {
                    numberOfQuestionGuessed++;
                }
            }
            this.NumberOfQuestionGuessed = numberOfQuestionGuessed;
        }
        public void CalcNumberOfItemsGuessedBasedOnCorrectAnswers(Dictionary<string, double[]> questionListWithStanderdDevAndAvg, List<Snapshot> snapshotTable)
        {
            int numberOfQuestionGuessed = 0;
            foreach (var question in this.TimeSpanOnEachItem)
            {
                //find z score. If < 1, it's a guess
                double averageTimeOntheQuestion = questionListWithStanderdDevAndAvg[question.Key][0];
                var timeStudentSpentToAnswer = Convert.ToDouble(question.Value.Ticks);
                bool isSmallerThanAverage = averageTimeOntheQuestion - timeStudentSpentToAnswer > 0;
                var zScore = ClaculteZScore(question, questionListWithStanderdDevAndAvg);

                if (zScore > 1 && isSmallerThanAverage && timeStudentSpentToAnswer != 0)
                {
                    numberOfQuestionGuessed++;
                    //// test
                    //var answer = snapshotTable.LastOrDefault(x=> x.ItemNum == question.Key).Response;
                    //var correctAnswer = correctAnswerList[question.Key];
                   
                    //if (answer != "Choice(s): " + correctAnswer)
                    //{
                    //    this.NumberOfWrongAnswerGuessed++;
                    //}
                    ////test end
                }
            }
            this.NumberOfQuestionGuessedBasedOnCorrectAnswers = numberOfQuestionGuessed;
        }
        /// <summary>
        /// If the time a student spent on answering aquetion was higher than the average, it is a guess 
        /// </summary>
        /// <param name="questionListWithStanderdDevAndAvg"></param>
        public void CalcNumberOfItemsUncertain(Dictionary<string, double[]> questionListWithStanderdDevAndAvg)
        {
            int numberOfQuestionUncertain = 0;
            foreach (var question in this.TimeSpanOnEachItem)
            {
                //find z score. If >= 2, it's a uncertain
                var averageTimeOntheQuestion = questionListWithStanderdDevAndAvg[question.Key][0];
                var timeStudentSpentToAnswer = question.Value.Ticks;
                bool isLargerThanAverage = averageTimeOntheQuestion - timeStudentSpentToAnswer < 0;
                var zScore = ClaculteZScore(question, questionListWithStanderdDevAndAvg);

                if (zScore > 1 && isLargerThanAverage)
                {
                    numberOfQuestionUncertain++;
                }
            }
            this.NumberOfQuestionUncertain = numberOfQuestionUncertain;
        }
        public void CalcNumberOfItemsUncertainBasedOnCorrectAnswers(Dictionary<string, double[]> questionListWithStanderdDevAndAvg, List<Snapshot> snapshotTable)
        {
            int numberOfQuestionUncertain = 0;
            foreach (var question in this.TimeSpanOnEachItem)
            {
                //find z score. If >= 2, it's a uncertain
                var averageTimeOntheQuestion = questionListWithStanderdDevAndAvg[question.Key][0];
                var timeStudentSpentToAnswer = question.Value.Ticks;
                bool isLargerThanAverage = averageTimeOntheQuestion - timeStudentSpentToAnswer < 0;
                var zScore = ClaculteZScore(question, questionListWithStanderdDevAndAvg);

                if (zScore > 1 && isLargerThanAverage)
                {
                    numberOfQuestionUncertain++;

                    // test
                    var answer = snapshotTable.LastOrDefault(x => x.ItemNum == question.Key).Response;
                    var correctAnswer = correctAnswerList[question.Key];

                    if (answer != "Choice(s): " + correctAnswer)
                    {
                        this.NumberOfWrongAnswerGuessed++;
                    }
                    //test end
                }
            }
            this.NumberOfQuestionUncertainBasedOnCorrectAnswers = numberOfQuestionUncertain;
        }
        /// <summary>
        /// For each question, this function compares the first answer with the final answer made by a student
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void CalcNumberOfItemsAnswersChangedFirstLast(List<Snapshot> snapshotTable)
        {
            int numberOfAnswersChanged = default(int);
            var snapshotsGroupedByItem = snapshotTable.GroupBy(x => x.ItemNum);

            foreach (var allSnapshotsForOneItem in snapshotsGroupedByItem)
            {
                var snapshotOffirstAnswer = allSnapshotsForOneItem.FirstOrDefault(x => !string.IsNullOrEmpty(x.Response));
                var lastAnswer = allSnapshotsForOneItem.LastOrDefault().Response;

                if (snapshotOffirstAnswer != null)
                {
                    if (snapshotOffirstAnswer.Response != lastAnswer)
                    {
                        numberOfAnswersChanged++;
                    }
                }                
            }
            this.NumberOfItemsAnswersChangedFirstLast = numberOfAnswersChanged;
        }
        public void CalcNumberOfItemsAnswersChanged(List<Snapshot> snapshotTable)
        {
            int numberOfAnswersChanged = default(int);
            var snapshotsGroupedByItem = snapshotTable.GroupBy(x => x.ItemNum);

            foreach (var allSnapshotsForOneItem in snapshotsGroupedByItem)
            {
                var lastAnswer = allSnapshotsForOneItem.LastOrDefault().Response;
                var isThereAnswerDifferentThanLastOne = allSnapshotsForOneItem.Where(x=> !string.IsNullOrEmpty(x.Response) && x.Response != lastAnswer).Count() > 0;

                if (isThereAnswerDifferentThanLastOne)
                {
                    numberOfAnswersChanged++;
                }
            }
            this.NumberOfItemsAnswersChanged = numberOfAnswersChanged;
        }
        /// <summary>
        /// This function count the number of times that a student change a question answer, but eventually stuck to the first one by 
        /// comparing the first and last answers first; then evaluate the answers in the visits in between to see if the student choose a different answer
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void CalcNumberOfQuestionsAnswerChangedButReturnToFirstOne(List<Snapshot> snapshotTable)
        {
            int numberOfItemsAnswerChanged = default(int);
            var snapshotsGroupedByItem = snapshotTable.GroupBy(x=> x.ItemNum);

            foreach (var allSnapshotsForOneItem in snapshotsGroupedByItem)
            {
                var snapOfFirstAnswerOfItem = allSnapshotsForOneItem.FirstOrDefault(x=> !string.IsNullOrEmpty(x.Response));
                var lastAnswer = allSnapshotsForOneItem.LastOrDefault().Response;

                if (snapOfFirstAnswerOfItem != null)
                {                
                    if (snapOfFirstAnswerOfItem.Response == lastAnswer)
                    {
                        var isThereAnotherAnswerInBetween = allSnapshotsForOneItem.FirstOrDefault(x => !string.IsNullOrEmpty(x.Response) && x.Response != lastAnswer);

                        if(isThereAnotherAnswerInBetween != null)
                        {
                            numberOfItemsAnswerChanged++;
                        }
                    }
                }                
            }
            this.NumberOfQuestionsAnswerChangedButReturnToFirstOne = numberOfItemsAnswerChanged;
        }
        /// <summary>
        /// In this function, the percentage of questions answered by a student is calculated by creating a counter that is incremented 
        /// throughout a loop when final answer is not blanck
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void CalculatePercentageOfQuestionsAswered(List<Snapshot> snapshotTable)
        {
            int numberOfQuestionsAnswered = default(int);
            var snapshotsGroupedByItem = snapshotTable.GroupBy(x => x.ItemNum);

            foreach (var allSnapshotsForOneItem in snapshotsGroupedByItem)
            {
                var finalAnswer = allSnapshotsForOneItem.LastOrDefault().Response;
                if(finalAnswer != null)
                {
                    if (!string.IsNullOrEmpty(finalAnswer))
                    {
                        numberOfQuestionsAnswered++;
                    }
                }
            }
            this.PercentageOfQuestionsAswered = ((double)numberOfQuestionsAnswered / snapshotsGroupedByItem.Count())*100;
        }
        //Section one ends
        /// Performance related features
        //Section Two starts        
        /// <summary>
        /// This function calculats the average time a student spent to answer a question correctly
        /// </summary>
        /// <param name="snapshotTable"></param>
        /// <param name="correctAnswerList"></param>
        public void CalculateAvgTimeOnCorrectAnswer(List<Snapshot> snapshotTable)
        {
            var timeSpanOnCorrectAnswerList = new List<TimeSpan>();
            this.TimeSpanOnCorrectAnswerForItem = new Dictionary<string, TimeSpan>();
            GeneralFuction(snapshotTable, ref timeSpanOnCorrectAnswerList, "time spent on correct answer");
            this.AvgTimeOnCorrectAnswers = CalculateAvgTime(timeSpanOnCorrectAnswerList);
        }
        /// <summary>
        /// This function calculats the average time a student spent to answer a question incorrectly
        /// </summary>
        /// <param name="snapshotTable"></param>
        /// <param name="correctAnswerList"></param>
        public void CalculateAvgTimeOnWrongAnswer(List<Snapshot> snapshotTable)
        {
            var timeSpanOnWrongAnswerList = new List<TimeSpan>();

            GeneralFuction(snapshotTable, ref timeSpanOnWrongAnswerList, "time spent on Wrong answer");
            this.AvgTimeOnWrongAnswers = CalculateAvgTime(timeSpanOnWrongAnswerList);
        }
        /// <summary>
        /// This a general function that implements the calculation of (CalcNumberOfItemsFromRightToWrong, CalcNumberOfItemsFromWrongToRight, CalcNumberOfItemsFromWrongToWrong) methods
        /// </summary>
        /// <param name="snapshotTable"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public int GeneralFunc2(List<Snapshot> snapshotTable, PassConditionForChangingAnswers condition)
        {
            var snapshotByItemNum = snapshotTable.GroupBy(x => x.ItemNum);
            int numberOfAttempts = 0;

            foreach (var snapsForEachItem in snapshotByItemNum)
            {
                var lastAnswer = snapsForEachItem.LastOrDefault().Response;
                var correctAnswer = "Choice(s): " + correctAnswerList[snapsForEachItem.Key];

                if (condition(snapsForEachItem, correctAnswer, lastAnswer))
                {
                    numberOfAttempts++;
                }
            }
            return numberOfAttempts;
        }
        /// <summary>
        /// The next three functins are passed as an argumant (to implement conditional statments) to the previous general function from (CalcNumberOfItemsFromRightToWrong, CalcNumberOfItemsFromWrongToRight, CalcNumberOfItemsFromWrongToWrong) methods
        /// </summary>
        /// <param name="snapsForEachItem"></param>
        /// <param name="correctAnswer"></param>
        /// <param name="lastAnswer"></param>
        /// <returns></returns>
        public delegate bool PassConditionForChangingAnswers(IGrouping<string, Snapshot> snapsForEachItem, string correctAnswer, string lastAnswer);
        public bool GetConditionForAnswersRightToWrong(IGrouping<string, Snapshot> snapsForEachItem, string correctAnswer, string lastAnswer)
        {
            var isLastAnswerWrong = lastAnswer != correctAnswer;
            var isThereCorrectAnswer = snapsForEachItem.FirstOrDefault(z => z.Response == correctAnswer) != null;

            return isLastAnswerWrong && isThereCorrectAnswer;
        }
        public bool GetConditionForAnswersWrongToRight(IGrouping<string, Snapshot> snapsForEachItem, string correctAnswer, string lastAnswer)
        {
            var isLastAnswerRight = lastAnswer == correctAnswer;
            var isThereWrongAnswer = snapsForEachItem.FirstOrDefault(z => z.Response != string.Empty && z.Response != correctAnswer) != null;

            return isLastAnswerRight && isThereWrongAnswer;
        }
        public bool GetConditionForAnswersWrongToWrong(IGrouping<string, Snapshot> snapsForEachItem, string correctAnswer, string lastAnswer)
        {            
            var isLastAnswerWrong = lastAnswer != correctAnswer;
            var isThereWrongAnswer = snapsForEachItem.FirstOrDefault(z => z.Response != string.Empty && z.Response != correctAnswer && z.Response != lastAnswer) != null;

            return isThereWrongAnswer && isLastAnswerWrong;
        }
        public bool GetConditionForAnswersRightToWrongToRight(IGrouping<string, Snapshot> snapsForEachItem, string correctAnswer, string lastAnswer)
        {
            var isLastAnswerRight = lastAnswer == correctAnswer;
            var firstAnswer = snapsForEachItem.FirstOrDefault(x=> !string.IsNullOrEmpty(x.Response));
            if (firstAnswer != null)
            {
                var isFirstAnswerRight = firstAnswer.Response == correctAnswer;
                var isLastAndFirstAnswersCorrect = isFirstAnswerRight && isLastAnswerRight;
                var isThereWrongAnswer = snapsForEachItem.FirstOrDefault(z => z.Response != string.Empty && z.Response != correctAnswer) != null;

                return isLastAndFirstAnswersCorrect && isThereWrongAnswer;
            }
            return false;
        }
        private bool GetConditionForAnswersRightToWrongFirsAndtLast(IGrouping<string, Snapshot> snapsForEachItem, string correctAnswer, string lastAnswer)
        {
            var isLastAnswerWrong = lastAnswer != correctAnswer;
            var isThereFirstAnswer = snapsForEachItem.FirstOrDefault(z => !string.IsNullOrEmpty(z.Response));

            if (isThereFirstAnswer != null && isLastAnswerWrong)
            {
                return correctAnswer == isThereFirstAnswer.Response;
            }
            return false;
        }
        private bool GetConditionForAnswersWrongToRightFirsAndtLast(IGrouping<string, Snapshot> snapsForEachItem, string correctAnswer, string lastAnswer)
        {
            var isLastAnswerRight = lastAnswer == correctAnswer;
            var snapshotOfFirstAnswer = snapsForEachItem.FirstOrDefault(z => !string.IsNullOrEmpty(z.Response));

            if (snapshotOfFirstAnswer != null && isLastAnswerRight)
            {
                return correctAnswer != snapshotOfFirstAnswer.Response;
            }
            return false;
        }
        private bool GetConditionForAnswersWrongToWrongFirsAndtLast(IGrouping<string, Snapshot> snapsForEachItem, string correctAnswer, string lastAnswer)
        {
            var isLastAnswerWrong = lastAnswer != correctAnswer;
            var snapshotOfFirstAnswer = snapsForEachItem.FirstOrDefault(z => !string.IsNullOrEmpty(z.Response));
            var snapshotOfLastAnswer = snapsForEachItem.LastOrDefault();

            if (snapshotOfFirstAnswer != null && isLastAnswerWrong)
            {
                if (lastAnswer != snapshotOfFirstAnswer.Response)
                {
                    return correctAnswer != snapshotOfFirstAnswer.Response;
                }
            }
            return false;
        }
        /// <summary>
        /// This function checks the final answer of a question; if it is wrong, it checks the previous visits' answers, and if it finds a correct answer amongst the previous visits, it consders the answer "from right to wrong".
        /// </summary>
        /// <param name="snapshotTable"></param>
        /// <param name="correctAnswerList"></param>
        public void CalcNumberOfItemsFromRightToWrong(List<Snapshot> snapshotTable)
        {
            PassConditionForChangingAnswers ConditionForAnswersRightToWrong = GetConditionForAnswersRightToWrong;
            this.NumberOfItemsAnswerWasFromRightToWrong = GeneralFunc2(snapshotTable, ConditionForAnswersRightToWrong);
        }
        /// <summary>
        /// This function checks the final answer of a question; if it is correct, it checks the previous visits' answers, and if it finds a wrong answer amongst the previous visits, it consders the answer "from wrong to right".
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void CalcNumberOfItemsFromWrongToRight(List<Snapshot> snapshotTable)
        {
            PassConditionForChangingAnswers ConditionForAnswersWrongToRight = GetConditionForAnswersWrongToRight;
            this.NumberOfItemsAnswerWasFromWrongToRight = GeneralFunc2(snapshotTable, ConditionForAnswersWrongToRight); ;
        }
        /// <summary>
        /// This function checks the final answer of a question; if it is wrong and no correct answer amongst the previous visits, it consders the answer "from wrong to wrong".
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void CalcNumberOfItemsFromWrongToWrong(List<Snapshot> snapshotTable)
        {
            PassConditionForChangingAnswers ConditionForAnswersWrongToWrong = GetConditionForAnswersWrongToWrong;
            this.NumberOfItemsAnswerWasFromWrongToWrong = GeneralFunc2(snapshotTable, ConditionForAnswersWrongToWrong);
        }
        /// <summary>
        /// This function checks the first and final answer of a question; if they are both correct, it checks the other visits' answers, and if it finds a wrong answer amongst the previous visits, it consders the answer "from right to wrong to right".
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void CalcNumberOfItemsFromRightToWrongToRight(List<Snapshot> snapshotTable)
        {
            PassConditionForChangingAnswers ConditionForAnswersWrongToRight = GetConditionForAnswersRightToWrongToRight;
            this.NumberOfItemsFromRightToWrongToRight = GeneralFunc2(snapshotTable, ConditionForAnswersWrongToRight); ;
        }
        public void CalcNumberOfItemsFromRightToWrongFirstAndLast(List<Snapshot> snapshotTable)
        {
            PassConditionForChangingAnswers ConditionForAnswersRightToWrongFirstAndLast = GetConditionForAnswersRightToWrongFirsAndtLast;
            this.NumberOfItemsAnswerWasFromRightToWrongFirstAndLast = GeneralFunc2(snapshotTable, ConditionForAnswersRightToWrongFirstAndLast);
        }
        public void CalcNumberOfItemsFromWrongToRightFirstAndLast(List<Snapshot> snapshotTable)
        {
            PassConditionForChangingAnswers ConditionForAnswersWrongToRightFirstAndLast = GetConditionForAnswersWrongToRightFirsAndtLast;
            this.NumberOfItemsAnswerWasFromWrongToRightFirstAndLast = GeneralFunc2(snapshotTable, ConditionForAnswersWrongToRightFirstAndLast);
        }
        public void CalcNumberOfItemsFromWrongToWrongFirstAndLast(List<Snapshot> snapshotTable)
        {
            PassConditionForChangingAnswers ConditionForAnswersWrongToWrongFirstAndLast = GetConditionForAnswersWrongToWrongFirsAndtLast;
            this.NumberOfItemsAnswerWasFromWrongToWrongFirstAndLast = GeneralFunc2(snapshotTable, ConditionForAnswersWrongToWrongFirstAndLast);
        }
        public void CalcOutOfTime()
        {
            var firstHalveSeventyQ = TimeSpanOnEachItem.Take(70);
            var secondHalveThirtyQ = TimeSpanOnEachItem.Skip(85);
            var averageOnFirstHalve = CalculateAvgTime(firstHalveSeventyQ.Select(x=> x.Value).ToList());
            var averageOnSecondHalve = CalculateAvgTime(secondHalveThirtyQ.Select(x=> x.Value).ToList());
            //var differenceBetweenFirstAndSecondHalves = (double)averageOnSecondHalve.Ticks / (double)averageOnFirstHalve.Ticks * 100;
            var differenceBetweenFirstAndSecondHalves = averageOnSecondHalve / averageOnFirstHalve * 100;

            this.OutOfTimePercentage = differenceBetweenFirstAndSecondHalves;
        }
        public void CalcTotalTimeSpent(List<Snapshot> snapshotTable)
        {
            var examStartAt = snapshotTable.FirstOrDefault().TimeStamp;
            var examEndAt = snapshotTable.LastOrDefault(x=> !triggerToEliminateTwo.Contains(x.Trigger.ToLower())).TimeStamp;
            //var totaltimeOnExam = Convert.ToDateTime(examEndAt) - Convert.ToDateTime(examStartAt);
            var totaltimeOnExam = Convert.ToDateTime(examEndAt).Ticks - Convert.ToDateTime(examStartAt).Ticks;

            this.TotalTimeOnExam = totaltimeOnExam;
        }
        public void ClcResultOfLasFiveAnswers(List<Snapshot> snapshotTable)
        {
            var lastFiveQuestions = snapshotTable.GroupBy(x=> x.ItemNum).Skip(85).Select(z=> z.LastOrDefault()).ToList();

            var numberOfCorrectAnswers = lastFiveQuestions.Count(x=> x.Response == "Choice(s): " + correctAnswerList[x.ItemNum]);
            
            this.ResultOfLastFiveAnswers = numberOfCorrectAnswers;
        }
    }
}
