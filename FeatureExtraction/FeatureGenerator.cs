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
        public int NumberOfMovementsThroughExam { set; get; }
        public TimeSpan AvgTimeSpanOnItem { set; get; }
        public TimeSpan AvgTimeSpanOnReviewingItem { set; get; }
        public int NumberOfQuestionGuessed { set; get; }
        public int NumberOfQuestionUncertain { set; get; }
        public int NumberOfItemsAnswresChanged { set; get; }
        public double PercentageOfQuestionsAswered { set; get; }
        public double averageNumberOfTimesQuestionVesitedBeforFinalAnswer { set; get; }
        public TimeSpan AvgTimeOnCorrectAnswers { set; get; }
        public TimeSpan AvgTimeOnWrongAnswers { set; get; }
        public int NumberOfItemsAnswerWasFromRightToWrong { set; get; }
        public int NumberOfItemsAnswerWasFromWrongToRight { set; get; }
        public int NumberOfItemsAnswerWasFromWrongToWrong { set; get; }
        public int NumberOfSkipsThroughTheTest { set; get; }
        private List<string> triggerToEliminate = new List<string>() { "10 minute save", "exam closed", "final", "final2", "exam start", "exam close", "exambegin" };
        internal Dictionary<string, TimeSpan> TimeSpanOnEachItem { set; get; }
        private Dictionary<string, string> correctAnswerList = Program.GetCorrectAnswerList();        

        public string PrepareForCsvFile()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($"{this.studentId}, " + 
                            $"{this.studentResult}, " + 
                            $"{this.NumberOfMovementsThroughExam}, " +
                            $"{this.AvgTimeSpanOnItem}, " +
                            $"{this.AvgTimeSpanOnReviewingItem}, " +
                            $"{this.NumberOfQuestionGuessed}, " +
                            $"{this.NumberOfQuestionUncertain}, " +
                            $"{this.NumberOfItemsAnswresChanged}, " +
                            $"{this.PercentageOfQuestionsAswered}, " +
                            $"{this.averageNumberOfTimesQuestionVesitedBeforFinalAnswer}, " +
                            $"{this.NumberOfSkipsThroughTheTest}, " +
                            $"{this.AvgTimeOnCorrectAnswers}, " +
                            $"{this.AvgTimeOnWrongAnswers}, " +                            
                            $"{this.NumberOfItemsAnswerWasFromWrongToRight}, " +
                            $"{this.NumberOfItemsAnswerWasFromRightToWrong}, " +
                            $"{this.NumberOfItemsAnswerWasFromWrongToWrong}\n");

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
        private TimeSpan CalculateAvgTime(List<TimeSpan> timeSpanList)
        {
            double DoubleAvgTicks = timeSpanList.Average(x=> x.Ticks);
            long longAvgTicks = Convert.ToInt64(DoubleAvgTicks);
            return new TimeSpan(longAvgTicks);
        }
        /// <summary>
        /// This method returns the number of movements through the exam by a student by counting the number of actions/triggers except the actions listed on the "triggerToEliminate" list
        /// </summary>
        /// <param name="snapshotTable"> List of all snapshots captured by the system for a student in the exam</param>
        public void ClaculateNumberOfMovementsThroughExam(List<Snapshot> snapshotTable)
        {
            int numberOfMovements;

            numberOfMovements = snapshotTable.Count(x => !triggerToEliminate.Contains(x.Trigger.ToLower()));
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

            foreach (var allSnapshotsForOneItem in snapshotsGroupedByItem)
            {
                List<int> indexOfsnapshotInSnapshotTable = new List<int>();
                var snapOfFirstAnswerOfItem = allSnapshotsForOneItem.FirstOrDefault();
                var snapOfLastVisitOfItem = allSnapshotsForOneItem.LastOrDefault();
                var snapOfFirstVisitOfFinalAnswerOfItem = snapOfFirstAnswerOfItem;                   

                switch (type)
                {
                    case "time spent on wrong answer":
                        if (snapOfLastVisitOfItem.Responce == "Choice(s): " + correctAnswerList[allSnapshotsForOneItem.Key]){
                            continue;
                        }break;
                    case "time spent on right answer":
                        if (snapOfLastVisitOfItem.Responce != "Choice(s): " + correctAnswerList[allSnapshotsForOneItem.Key]){
                            continue;
                        }break;
                }
                foreach (var snapshot in allSnapshotsForOneItem)
                {
                    if (snapshot.Responce != snapOfFirstVisitOfFinalAnswerOfItem.Responce)
                    {
                        snapOfFirstVisitOfFinalAnswerOfItem = snapshot;
                    }
                    indexOfsnapshotInSnapshotTable.Add(snapshot.Index);
                }
                indexOfsnapshotInSnapshotTable.RemoveAll(x => x > snapOfFirstVisitOfFinalAnswerOfItem.Index); // Remove snapshots recorded after the final answer
                timeSabanList.Add(CalculateTimeSpaneOnItem(indexOfsnapshotInSnapshotTable, snapshotTable));
                if (type == "add the calculated time to the timing list")
                {
                    this.TimeSpanOnEachItem.Add(snapshotTable[indexOfsnapshotInSnapshotTable[0]].ItemNum, timeSabanList[timeSabanList.Count - 1]);
                }
            }
        }
        /// <summary>
        /// This method returns the average time a student spent on a question. Basicly, it calculates the time on each visit to a question recorded by the system from the first visit of the final answer until the last visit to the question
        /// </summary>
        /// <param name="snapshotTable">List of all snapshots captured by the system for a student in the exam</param>
        public void AvgTimeOnItem(List<Snapshot> snapshotTable)
        {
            var timeSpanOnEachItemList = new List<TimeSpan>();
            this.TimeSpanOnEachItem = new Dictionary<string, TimeSpan>();

            GeneralFuction(snapshotTable, ref timeSpanOnEachItemList, "add the calculated time to the timing list");
            this.AvgTimeSpanOnItem = CalculateAvgTime(timeSpanOnEachItemList);
        }
        /// <summary>
        /// It calculates the average time a student spent in each visit to a qustion after the first visit in which a student chose an answer
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void AvgTimeOnReviewingItem(List<Snapshot> snapshotTable)
        {
            var snapshotsGroupedByItem = snapshotTable.GroupBy(x => x.ItemNum);
            var timeSpanOnEachItem = new List<TimeSpan>();

            foreach (var snapByItemNum in snapshotsGroupedByItem)
            {
                var snapOfFirstAnswerOfItem = snapByItemNum.FirstOrDefault(x => !string.IsNullOrEmpty(x.Responce));

                if (snapOfFirstAnswerOfItem != null)
                {
                    var index = snapByItemNum.Where(x => x.Index > snapOfFirstAnswerOfItem.Index && 
                                                   !triggerToEliminate.Contains(x.Trigger.ToLower())).Select(x => x.Index).ToList();
                    timeSpanOnEachItem.Add(CalculateTimeSpaneOnItem(index, snapshotTable));
                }                
            }
            this.AvgTimeSpanOnReviewingItem = CalculateAvgTime(timeSpanOnEachItem);
        }
        /// <summary>
        /// It calculates the average number of times a student visited a question before he/she made the final answer
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void AvgNumOfTimesItemVisitedBeforeFinalAnswer(List<Snapshot> snapshotTable)
        {
            var snapshotByItemNum = snapshotTable.GroupBy(x => x.ItemNum);
            var indexNumOfTimesOfVisitBeforFinalAnswer = new List<int>();

            foreach (var snapsForEachItem in snapshotByItemNum)
            {
                var responseOfFinalAnswer = snapsForEachItem.LastOrDefault().Responce;
                var precedentSnapOfFinalAnswerSnap = snapsForEachItem.LastOrDefault(x=> x.Responce != responseOfFinalAnswer);
                var allSnapshotOfVisitsBeforFinalAnswer = snapsForEachItem.Where(x=> x.Index <= precedentSnapOfFinalAnswerSnap.Index);

                if (precedentSnapOfFinalAnswerSnap != null)
                {
                    indexNumOfTimesOfVisitBeforFinalAnswer.Add(allSnapshotOfVisitsBeforFinalAnswer.Where(x=> snapshotTable[x.Index+1].ItemNum != x.ItemNum).Count());
                }
            }
            this.averageNumberOfTimesQuestionVesitedBeforFinalAnswer = indexNumOfTimesOfVisitBeforFinalAnswer.Average();
        }
        /// <summary>
        /// Counting the total number of skips through the exam by checking the first visit to an item; if there is no answer in the first visit the function reports that as a skip 
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void NumberOfSkips(List<Snapshot> snapshotTable)
        {
            int skipCounter;
            var snapshotOfItemSkipped = snapshotTable.Where(x=> x.Responce == string.Empty && !triggerToEliminate.Contains(x.Trigger) && snapshotTable[x.Index + 1].ItemNum != x.ItemNum);
            skipCounter = snapshotOfItemSkipped.Count();

            this.NumberOfSkipsThroughTheTest = skipCounter;
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
            double standerdDeviation = questionListWithStanderdDevAndAvg[questionWithTimeStudentSpentOn.Key][1];
            double zScore = Math.Abs(questionWithTimeStudentSpentOn.Value.Ticks - avgOnQuestion) / standerdDeviation;

            return zScore;
        }
        /// <summary>
        /// If the time a student spent on answering aquetion was lower than the average, it is a guess 
        /// </summary>
        /// <param name="questionListWithStanderdDevAndAvg"></param>
        public void NumberOfItemsGuessed(Dictionary<string, double[]> questionListWithStanderdDevAndAvg)
        {
            int numberOfQuestionGuessed = 0;
            foreach (var question in this.TimeSpanOnEachItem)
            {
                //find z score. If < 1, it's a guess
                var averageTimeOntheQuestion = questionListWithStanderdDevAndAvg[question.Key][0];
                var timeStudentSpentToAnswer = question.Value.Ticks;
                bool isSmallerThanAverage = averageTimeOntheQuestion - timeStudentSpentToAnswer > 0;
                var zScore = ClaculteZScore(question, questionListWithStanderdDevAndAvg);

                if (zScore > .8 && isSmallerThanAverage)
                {
                    numberOfQuestionGuessed++;
                }
            }
            this.NumberOfQuestionGuessed = numberOfQuestionGuessed;
        }
        /// <summary>
        /// If the time a student spent on answering aquetion was higher than the average, it is a guess 
        /// </summary>
        /// <param name="questionListWithStanderdDevAndAvg"></param>
        public void NumberOfItemsUncertain(Dictionary<string, double[]> questionListWithStanderdDevAndAvg)
        {
            int numberOfQuestionUncertain = 0;
            foreach (var question in this.TimeSpanOnEachItem)
            {
                //find z score. If >= 2, it's a uncertain
                var averageTimeOntheQuestion = questionListWithStanderdDevAndAvg[question.Key][0];
                var timeStudentSpentToAnswer = question.Value.Ticks;
                bool isLargerThanAverage = averageTimeOntheQuestion - timeStudentSpentToAnswer < 0;
                var zScore = ClaculteZScore(question, questionListWithStanderdDevAndAvg);

                if (zScore >= 2 && isLargerThanAverage)
                {
                    numberOfQuestionUncertain++;
                }
            }
            this.NumberOfQuestionUncertain = numberOfQuestionUncertain;
        }
        /// <summary>
        /// For each question, this function compares the first answer with the final answer made by a student
        /// </summary>
        /// <param name="snapshotTable"></param>
        public void CalcNumberOfItemsAnswresChanged(List<Snapshot> snapshotTable)
        {
            int NumberOfAnswersChanged = default(int);
            var snapshotsGroupedByItem = snapshotTable.GroupBy(x => x.ItemNum);

            foreach (var snapByItemNum in snapshotsGroupedByItem)
            {
                var firstAnswer = snapByItemNum.FirstOrDefault(x => !string.IsNullOrEmpty(x.Responce));
                var lastAnswer = snapByItemNum.LastOrDefault().Responce;

                if (firstAnswer != null)
                {
                    if (firstAnswer.Responce != lastAnswer)
                    {
                        NumberOfAnswersChanged++;
                    }
                }                
            }
            this.NumberOfItemsAnswresChanged = NumberOfAnswersChanged;
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
                var finalAnswer = allSnapshotsForOneItem.LastOrDefault().Responce;
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

            GeneralFuction(snapshotTable, ref timeSpanOnCorrectAnswerList, "time spent on wrong answer");
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

            GeneralFuction(snapshotTable, ref timeSpanOnWrongAnswerList, "time spent on right answer");
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
                var lastAnswer = snapsForEachItem.LastOrDefault().Responce;
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
            var isEqual = lastAnswer != correctAnswer;
            var isThereCorrectAnswer = snapsForEachItem.FirstOrDefault(z => z.Responce == correctAnswer) != null;

            return isEqual && isThereCorrectAnswer;
        }
        public bool GetConditionForAnswersWrongToRight(IGrouping<string, Snapshot> snapsForEachItem, string correctAnswer, string lastAnswer)
        {
            var isEqual = lastAnswer == correctAnswer;
            var isThereWrongAnswer = snapsForEachItem.FirstOrDefault(z => z.Responce != string.Empty && z.Responce != correctAnswer) != null;

            return isEqual && isThereWrongAnswer;
        }
        public bool GetConditionForAnswersWrongToWrong(IGrouping<string, Snapshot> snapsForEachItem, string correctAnswer, string lastAnswer)
        {            
            var isEqual = lastAnswer != correctAnswer;
            var lastAnswerSnap = snapsForEachItem.LastOrDefault();
            var isThereWrongAnswer = snapsForEachItem.FirstOrDefault(z => z.Responce != string.Empty && z.Responce != correctAnswer && z != lastAnswerSnap && z.Responce != lastAnswer) != null;

            return isThereWrongAnswer && isEqual;
        }
        /// <summary>
        /// This function checks the final answer of a question; if it is wrong, it checks the previous visits' answers, and if it finds a correct answer amongst the previous visits, it consders the answer "from right to wrong".
        /// </summary>
        /// <param name="snapshotTable"></param>
        /// <param name="correctAnswerList"></param>
        public void CalcNumberOfItemsFromRightToWrong(List<Snapshot> snapshotTable)
        {
            PassConditionForChangingAnswers ConditionForAnswersRightToWron = GetConditionForAnswersRightToWrong;
            this.NumberOfItemsAnswerWasFromRightToWrong = GeneralFunc2(snapshotTable, ConditionForAnswersRightToWron);
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
    }
}
