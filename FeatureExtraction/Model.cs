﻿using System;
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
        /// "This fuction gets students' snapshots from a csv file and represents them in a data structure for each student
        /// </summary>
        /// <param name="path">Where the data is located</param>
        /// <param name="students">An object of type student that has an id and a snapshot list</param>
        public void ConstructStudent(string path, ref List<Student> students, Dictionary<string, string> studentResultsList)
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
        /// This function gets all the exam questions with the correct answers and stores them in a dectionary
        /// </summary>
        /// <param name="pathForCorrectAnswers">This is the directory path of the file that contains the questions and their correct answers</param>
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
        /// This function gets all the exam questions with the correct answers and stores them in a dectionary
        /// </summary>
        /// <param name="pathForCorrectAnswers">This is the directory path of the file that contains the questions and their correct answers</param>
        /// <param name="correctAnswerList">The key is the question number and the value is the correct answer</param>
        public void ConstructStudentGroup(string pathForStudentGroup, ref List<string> studentGroupIds)
        {
            using (var stream = new StreamReader(pathForStudentGroup))
            {
                string headerLine = stream.ReadLine();
                string row;

                while ((row = stream.ReadLine()) != null)
                {
                    studentGroupIds.Add(row);
                }
            }
        }
        /// <summary>
        /// This function exports the features extracted into a csv file
        /// </summary>
        /// <param name="featureListForAllStudents"> This is a list comprised of all students with their features</param>
        public void ExportToCsvFile(List<FeatureGenerator> featureListForAllStudents)
        {
            File.WriteAllText("../../../featuresGenerated.csv", string.Empty);
            StringBuilder csv = new StringBuilder();
            csv.Append($"Student id, " +
                        $"% Result, " +
                        $"% Total t, " +
                        $"Total Movements, " +
                        $"Ran out of time?, " +
                        $"Num of Right Ans of Las Five Q, " +
                        $"Avg t to Answering, " +
                        $"Avg t to Reviewg, " + 
                        $"Avg t on Reviewing After Final Ans, " + 
                        $"Avg t on Q, " +
                        $"Num Q Gussed, " +
                        $"Num Q Gussed Based On Right Ans, " +
                        $"Num Q Uncertin, " +
                        $"Num Q Uncertin Based On Right Ans, " +
                        $"Num Q Ans Changed Compare F & L Ans, " +
                        $"Num Q Ans Changed, " +
                        $"Num Q Ans Changed But Return First , " +
                        $"% Q Answered, " +
                        $"Avg Q Visits Before Final Ans, " +
                        $"Avg Q Visits After First Ans, " +
                        $"Avg Q Visits All Times, " +
                        $"Avg Visits With Blanck Ans, " +
                        $"Total Visits To Q, " +
                        $"Num Skips," +
                        $"Avg t Correct Ans, " +
                        $"Avg t Wrong Ans," +
                        $"Num W to R, " +
                        $"Num R to W, " +
                        $"Num W to W, " +
                        $"Num R to W Compare F & L Ans, " +
                        $"Num W to R Compare F & L Ans, " +
                        $"Num W to W Compare F & L Ans, " +
                        $"Num W Ans guessed, " +
                        $"Num R to W to R\n");            
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
        /// <param name="pathForStudentResultsFile">This is the path of the file that contains the results</param>
        /// <param name="studentResultList">This is the dectionary used to hold students' results. The key is student id and the value is the final result</param>
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
