using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureExtraction
{
    class Snapshot
    {
        public string ItemNum { set; get; }
        public string RandomizedItem { set; get; }
        public string ItemType { set; get; }
        public string TimeStamp { set; get; }
        public string Trigger { set; get; }
        public string Response { set; get; }
        public int Index { set; get; }

        public void AddRecord(List<string> line, int index)
        {
            this.ItemNum = line[1];
            this.RandomizedItem = line[2];
            this.ItemType = line[3];
            this.TimeStamp = line[4];
            this.Trigger = line[5];
            this.Response = line[6];
            this.Index = index;
        }
    }
}
