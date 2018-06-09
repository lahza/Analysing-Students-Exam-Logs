using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FeatureExtraction
{
    class Program
    {
        private static List<FeatureGenerator>  featureListForAllStudents = new List<FeatureGenerator>();
        private static Dictionary<string, double[]> questionListWithStanderdDevAndAvg = new Dictionary<string, double[]>();
        private static Dictionary<string, double[]> questionListWithStanderdDevAndAvgBasedOnCorrectAnswers = new Dictionary<string, double[]>();
        /// <summary>
        /// This fuction returns a dictionary of the nmber of questions with their correct answers (Key: number of question, Value: the correct answer)
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetCorrectAnswerList()
        {
            var pathForCorrectAnswersFile = "../../../Correct answers.csv";
            var correctAnswerList = new Dictionary<string, string>();
            var constructDataStruct = new Model();
            constructDataStruct.ConstructCorrectAnswerList(pathForCorrectAnswersFile, ref correctAnswerList);

            return correctAnswerList;
        }
        /// <summary>
        /// This function returns the result of the student exam (Key: student id, Value: student result)
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, string> GetStudentResultsList(ref Model constructDataStruct)
        {
            var pathForStudentResultsFile = "../../../Students results only.csv";
            var StudentResultsList = new Dictionary<string, string>();   
            
            constructDataStruct.ConstructStudentResultList(pathForStudentResultsFile, ref StudentResultsList);

            return StudentResultsList;
        }
        private static List<string> GetStudentGroup(ref Model constructDataStruct)
        {
            var pathForStudentGroupFile = "../../../group two.csv";
            var studentGroupIds = new List<string>();

            constructDataStruct.ConstructStudentGroup(pathForStudentGroupFile, ref studentGroupIds);

            return studentGroupIds;
        }
        /// <summary>
        /// This function sets up the standerd deviation of the time spent by students to answer a question
        /// </summary>
        /// <param name="allTimespanForItem"></param>
        /// <param name="question"></param>
        private static void SetStudentsStanderdDeviationOfAnswering(List<TimeSpan> allTimespanForItem, string question, string type)
        {
            double average = allTimespanForItem.Average(x => x.Ticks);
            var variance = allTimespanForItem.Sum(x => Math.Pow(x.Ticks - average, 2));
            var standerdDeviation = Math.Sqrt(variance / allTimespanForItem.Count);
            switch (type)
            {
                case "All":
                    questionListWithStanderdDevAndAvg.Add(question, new double[2] { average, standerdDeviation });
                    break;
                case "Based on correct answers":
                    questionListWithStanderdDevAndAvgBasedOnCorrectAnswers.Add(question, new double[2] { average, standerdDeviation });
                    break;
            }
        }
        /// <summary>
        /// This function returns a list of students who took the exam with their data (snapshots, id, result)
        /// </summary>
        /// <returns></returns>
        private static List<Student> GetStudentList(ref Model constructDataStruct)
        {
            var pathForStudentFile = "../../../DeidentifiedResultsEditedF.csv";
            var students = new List<Student>();
            var StudentResultsList = GetStudentResultsList(ref constructDataStruct);
            constructDataStruct.ConstructStudent(pathForStudentFile, ref students, StudentResultsList);

            return students;
        }
        static void Main(string[] args)
        {
            var constructDataStruct = new Model();
            var students = GetStudentList(ref constructDataStruct);
            var questionList = GetCorrectAnswerList().Keys.ToList();
            var studentGroupTwo = GetStudentGroup(ref constructDataStruct);
            var studentGroup = students.Where(x => studentGroupTwo.Contains(x.Id));

            foreach (var student in studentGroup/*students*//*.Skip(447)*/)
            {
                var featureForEachStudent = new FeatureGenerator();
                var snapshot = student.Snapshots;
                featureForEachStudent.studentId = student.Id;
                featureForEachStudent.studentResult = student.Result;
                
                //Feature generating starts
                featureForEachStudent.ClaculateNumberOfMovementsThroughExam(snapshot); // checked2
                featureForEachStudent.CalcAvgTimeOnItemToGetTheFinalAnswer(snapshot); //checked2
                featureForEachStudent.AvgTimeOnReviewingItem(snapshot); //checked2
                featureForEachStudent.CalcAvgTimeSpentOnReviewingItemAfterFinalAnswer(snapshot);
                featureForEachStudent.CalcAvgTimeSpentOnItemForAllVisits(snapshot); //checked2
                featureForEachStudent.CalcAvgNumberOfVisitsBeforeFinalAnswer(snapshot); //checked2
                featureForEachStudent.CalcAvgNumberOfVisitsAfterFirstAnswer(snapshot);
                featureForEachStudent.CalcAvgVisitsToItemAllTimes(snapshot); 
                featureForEachStudent.CalcAvgNumberOfVisitsWithNoAnswer(snapshot); 
                featureForEachStudent.CalcTotalNumberOfVisitsToAllQuestions(snapshot);
                featureForEachStudent.CalcNumberOfItemsAnswersChangedFirstLast(snapshot); //checked3
                featureForEachStudent.CalcNumberOfItemsAnswersChanged(snapshot); 
                featureForEachStudent.CalcNumberOfQuestionsAnswerChangedButReturnToFirstOne(snapshot);
                featureForEachStudent.CalculatePercentageOfQuestionsAswered(snapshot); //checked2
                featureForEachStudent.CalculateAvgTimeOnCorrectAnswer(snapshot); //checked
                featureForEachStudent.CalculateAvgTimeOnWrongAnswer(snapshot); //checked                
                featureForEachStudent.CalcNumberOfItemsFromRightToWrong(snapshot); //checked3
                featureForEachStudent.CalcNumberOfItemsFromWrongToRight(snapshot); //checked3
                featureForEachStudent.CalcNumberOfItemsFromWrongToWrong(snapshot); //checked3
                featureForEachStudent.CalcNumberOfItemsFromRightToWrongFirstAndLast(snapshot); 
                featureForEachStudent.CalcNumberOfItemsFromWrongToRightFirstAndLast(snapshot); 
                featureForEachStudent.CalcNumberOfItemsFromWrongToWrongFirstAndLast(snapshot); 
                featureForEachStudent.CalcNumberOfItemsFromRightToWrongToRight(snapshot);
                featureForEachStudent.NumberOfSkips(snapshot); //checked
                featureListForAllStudents.Add(featureForEachStudent);
                //New
                featureForEachStudent.CalcTotalTimeSpent(snapshot);
                featureForEachStudent.CalcOutOfTime();
                featureForEachStudent.ClcResultOfLasFiveAnswers(snapshot);
            }            
            foreach (var question in questionList)
            {
                var allTimespanForItem = featureListForAllStudents.Select(x => x.TimeSpanOnEachItem).Select(z => z[question]).ToList();             
                var allTimespanForItemForStudentAnswerCorrectly = featureListForAllStudents.Select(x => x.TimeSpanOnCorrectAnswerForItem).Where(w=> w.Keys.Contains(question)).Select(z => z[question]).ToList();

                SetStudentsStanderdDeviationOfAnswering(allTimespanForItem,  question, "All");
                SetStudentsStanderdDeviationOfAnswering(allTimespanForItemForStudentAnswerCorrectly, question, "Based on correct answers");
            }
            foreach (var studentFeature in featureListForAllStudents)
            {
                studentFeature.CalcNumberOfItemsGuessed(questionListWithStanderdDevAndAvg); //checked
                studentFeature.CalcNumberOfItemsUncertain(questionListWithStanderdDevAndAvg); //checked
                //test
                var student = /*students*/studentGroup.FirstOrDefault(x=> x.Id == studentFeature.studentId);
                //test end
                studentFeature.CalcNumberOfItemsGuessedBasedOnCorrectAnswers(questionListWithStanderdDevAndAvgBasedOnCorrectAnswers, /*test param*/student.Snapshots);
                studentFeature.CalcNumberOfItemsUncertainBasedOnCorrectAnswers(questionListWithStanderdDevAndAvgBasedOnCorrectAnswers, /*test param*/student.Snapshots); 

            }
            constructDataStruct.ExportToCsvFile(featureListForAllStudents);

            Console.ReadLine();
        }
    }
}
