using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    [Serializable]
    public class FlatFileRecord
    {
        public string IntersectionName { get; set; }
        public DateTime DateTime { get; set; }
        public string Approach { get; set; }
        public string Direction { get; set; }
        public string CommuterClass { get; set; }
        public int Count { get; set; }

        public FlatFileRecord()
        {

        }

        public FlatFileRecord(string intersectionName, DateTime dateTime, string approach, string direction, string commuterClass, int count)
        {
            this.IntersectionName = intersectionName;
            this.Approach = approach;
            this.DateTime = dateTime;
            this.Direction = direction;
            this.CommuterClass = commuterClass;
            this.Count = count;
        }

        public override string ToString()
        {
            string output = ",";
            output += IntersectionName + ",";
            output += DateTime.ToShortDateString() + ",";
            output += Approach + ",";
            output += Direction + ",";
            output += CommuterClass + ",";
            output += DateTime.TimeOfDay.ToString() + ",";
            output += Count + "";
            return output;
        }

    }
}
