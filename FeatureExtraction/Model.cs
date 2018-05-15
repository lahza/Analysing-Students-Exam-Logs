using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FeatureExtraction
{
    class Model
    {
        /// <summary>
        /// "This fuction gets students' snapshots from an csv file and represents them in a data structure for each student
        /// </summary>
        /// <param name="path">Where the data is located</param>
        /// <param name="students">An object of type student that has an id and snapshot list</param>
        public void ConstructStudent(string path, List<Student> students, Dictionary<string, string> studentResultsList)
        {
            int lastItem = students.Count;
            List<string> SplitLine(string line)
            {
                return line.Split(new char[] { ',' }, StringSplitOptions.None).ToList();
            }
            void Add(List<string> record, List<Student> StudentList, int index)
            {
                var snapShot = new Snapshot();
                
                StudentList.Add(new Student());
                StudentList[index].Id = record[0]; 
                StudentList[index].Result = studentResultsList[record[0]]; 
                snapShot.AddRecord(record, 0);
                StudentList[index].Snapshots = new List<Snapshot>() {snapShot};
            }

            using (var stream = new StreamReader(path))
            {
                string headerLine = stream.ReadLine();
                string firstLine = stream.ReadLine();
                string currentLine;
                int index = 0;

                Add(SplitLine(firstLine), students, lastItem);
                while ((currentLine = stream.ReadLine()) != null)
                {
                    index++;
                    var record = SplitLine(currentLine);
                    var snapShot = new Snapshot();

                    if (students[lastItem].Id != record[0])
                    {
                        lastItem++;
                        Add(record, students, lastItem);
                        index = 0;
                    }
                    else
                    {
                        snapShot.AddRecord(record, index);
                        students[lastItem].Snapshots.Add(snapShot);
                    }                        
                }
            }
        }
        /// <summary>
        /// This function gets all the exam question with the correct answers and store them in a dectionary
        /// </summary>
        /// <param name="pathForCorrectAnswers">This the directory path of the file that contains the question and their correct answers</param>
        /// <param name="correctAnswerList">The key is the question number and the value is the correct answer</param>
        public void ConstructCorrectAnswerList(string pathForCorrectAnswers, ref Dictionary<string, string> correctAnswerList)
        {
            using (var stream = new StreamReader(pathForCorrectAnswers))
            {
                string headerLine = stream.ReadLine();
                string row;
                
                while((row = stream.ReadLine()) != null)
                {
                    var tempList = row.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    correctAnswerList.Add(tempList[0], tempList[1]);
                }
            }
        }
        /// <summary>
        /// The fuction export the features extracted into a csv file
        /// </summary>
        /// <param name="featureListForAllStudents"> This is a list comprmised of all students with their featurs</param>
        public void ExportToCsvFile(List<FeatureGenerator> featureListForAllStudents)
        {
            File.WriteAllText("../../../featuresGenerated.csv", string.Empty);
            StringBuilder csv = new StringBuilder();
            csv.Append($"Student id, " +
                        $"% Result, " +
                        $"Total Movements, " +
                        $"Avg t to Answering, " +
                        $"Avg t to Reviewg, " +
                        $"Num Q Gussed, " +
                        $"Num Q Uncertin, " +
                        $"Num Q Ans Changed, " +
                        $"% Q Answered, " +
                        $"Avg Q Visits Before Final Ans, " +
                        $"Num Skips," +
                        $"Avg t Correct Ans, " +
                        $"Avg t Wrong Ans," +
                        $"Num W to R," +
                        $"Num R to W," +
                        $"Num W to W \n");            
            foreach (var featuresForOneStudent in featureListForAllStudents)
            {
                var features = featuresForOneStudent.PrepareForCsvFile();
                csv.Append(features);
            }
            File.AppendAllText("../../../featuresGenerated.csv", csv.ToString());
        }
        /// <summary>
        /// This function gets the results of students in the test and costructs a dectionary to store them
        /// </summary>
        /// <param name="pathForStudentResultsFile">This is the path of file that contains the results</param>
        /// <param name="studentResultList">This is the dectionary used to hold students results. The key is student id and the value is the number the final result</param>
        public void ConstructStudentResultList(string pathForStudentResultsFile, ref Dictionary<string, string> studentResultList)
        {
            using (var stream = new StreamReader(pathForStudentResultsFile))
            {
                string headerLine = stream.ReadLine();
                string row;

                while ((row = stream.ReadLine()) != null)
                {
                    var tempList = row.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    studentResultList.Add(tempList[0], tempList[1]);
                }
            }
        }
    }
}
