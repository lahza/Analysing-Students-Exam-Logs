using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureExtraction
{
    /// <summary>
    /// Each student represnts an object of this class with his/her id, exam resut, and snapshots of the exam
    /// </summary>
    class Student
    {
        public string Id { set; get; }
        public List<Snapshot> Snapshots {set; get; } // All snapshots for one student are aggregated in a list
        public string Result { set; get; }

    }
}
