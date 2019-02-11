using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    class FlatFileRecord
    {
        public string IntersectionName { get; set; }
        public DateTime DateTime { get; set; }
        public string fromApproach { get; set; }
        public string Direction { get; set; }
        public string CommuterClass { get; set; }
        public int Count { get; set; }
        public string toApproach { get; set; }

        public FlatFileRecord()
        {

        }

        public FlatFileRecord(string intersectionName, DateTime dateTime, string approach, string direction, string commuterClass, int count)
        {
            this.IntersectionName = intersectionName;
            this.DateTime = dateTime;
            this.fromApproach = approach;
            this.Direction = direction;
            this.CommuterClass = commuterClass;
            this.Count = count;
        }

    }
}
