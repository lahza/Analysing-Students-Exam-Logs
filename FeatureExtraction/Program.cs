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
        public static Dictionary<string, string> GetCorrectAnswerList()
        {
            var correctAnswerList = new Dictionary<string, string>();
            var pathForCorrectAnswersFile = "../../../Correct answers.csv";
            var constructDataStruct = new Model();
            constructDataStruct.ConstructCorrectAnswerList(pathForCorrectAnswersFile, ref correctAnswerList);

            return correctAnswerList;
        }
        private static Dictionary<string, string> GetStudentResultsList()
        {
            var StudentResultsList = new Dictionary<string, string>();
            var pathForStudentResultsFile = "../../../Students results only.csv";
            var constructDataStruct = new Model();
            constructDataStruct.ConstructStudentResultList(pathForStudentResultsFile, ref StudentResultsList);

            return StudentResultsList;
        }
        static void Main(string[] args)
        {
            string path = "../../../DeidentifiedResultsEditedF.csv";
            var students = new List<Student>();
            var constructDataStruct = new Model();
            var featureListForAllStudents = new List<FeatureGenerator>();
            var questionListWithStanderdDevAndAvg = new Dictionary<string, double[]>();
            var questionList = GetCorrectAnswerList().Keys.ToList();
            var StudentResultsList = GetStudentResultsList();

            constructDataStruct.ConstructStudent(path, students, StudentResultsList);
            
            foreach (var student in students)
            {
                var featureForEachStudent = new FeatureGenerator();
                var snapshot = student.Snapshots;
                featureForEachStudent.studentId = student.Id;
                featureForEachStudent.studentResult = student.Result;
                
                //Feature generating starts
                featureForEachStudent.ClaculateNumberOfMovementsThroughExam(snapshot); // checked2
                featureForEachStudent.AvgTimeOnItem(snapshot); //checked2
                featureForEachStudent.AvgTimeOnReviewingItem(snapshot); //checked2
                featureForEachStudent.AvgNumOfTimesItemVisitedBeforeFinalAnswer(snapshot); //checked2
                featureForEachStudent.CalcNumberOfItemsAnswresChanged(snapshot); //checked3
                featureForEachStudent.CalculatePercentageOfQuestionsAswered(snapshot); //checked2
                featureForEachStudent.CalculateAvgTimeOnCorrectAnswer(snapshot); //checked
                featureForEachStudent.CalculateAvgTimeOnWrongAnswer(snapshot); //checked                
                featureForEachStudent.CalcNumberOfItemsFromRightToWrong(snapshot); //checked3
                featureForEachStudent.CalcNumberOfItemsFromWrongToRight(snapshot); //checked3
                featureForEachStudent.CalcNumberOfItemsFromWrongToWrong(snapshot); //checked3
                featureForEachStudent.NumberOfSkips(snapshot); //checked
                featureListForAllStudents.Add(featureForEachStudent);
            }

            foreach (var question in questionList)
            {
                var allTimespanForItem = featureListForAllStudents.Select(x => x.TimeSpanOnEachItem).Select(z => z[question]).ToList();
                double average = allTimespanForItem.Average(x=> x.Ticks);
                var variance = allTimespanForItem.Sum(x=> Math.Pow(x.Ticks - average, 2));
                var standerdDeviation = Math.Sqrt(variance/allTimespanForItem.Count);

                questionListWithStanderdDevAndAvg.Add(question, new double[2] {average, standerdDeviation});
            }
            foreach (var studentFeature in featureListForAllStudents)
            {
                studentFeature.NumberOfItemsGuessed(questionListWithStanderdDevAndAvg); //checked
                studentFeature.NumberOfItemsUncertain(questionListWithStanderdDevAndAvg); //checked
            }
            constructDataStruct.ExportToCsvFile(featureListForAllStudents);
            visioConsole.ReadLine();
        }
    }
}
