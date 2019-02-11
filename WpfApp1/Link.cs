using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace WpfApp1
{
    [Serializable]
    public class Link
    {
        public Intersection StartIntersection { get; set; }
        public Intersection EndIntersection { get; set; }
        public Line Line { get; set; }

        public double orientation()
        {
            var dy = Line.Y2 - Line.Y1;
            var dx = Line.X2 - Line.X1;

            if (dx == 0) return 0;
            return Math.Atan(dy/ dx);
        }

        public Link()
        {

        }

        public Link(Intersection start, Intersection end, Line inputLine)
        {
            StartIntersection = start;
            EndIntersection = end;
            Line = inputLine;
        }

       public double StartInflow()
        {
            if (this == StartIntersection.GetEastLink())
            {
                return StartIntersection.EastInFlow();
            }

            if (this == StartIntersection.GetWestLink())
            {
                return StartIntersection.WestInFlow();
            }

            if (this == StartIntersection.GetNorthLink())
            {
                return StartIntersection.NorthInFlow();
            }

            if (this == StartIntersection.GetSouthLink())
            {
                return StartIntersection.SouthInFlow();
            }
            return double.NaN;
        }

        public double StartOutflow()
        {

            if (this == StartIntersection.GetEastLink())
            {
                return StartIntersection.EastOutFlow();
            }

            if (this == StartIntersection.GetWestLink())
            {
                return StartIntersection.WestOutFlow();
            }

            if (this == StartIntersection.GetNorthLink())
            {
                return StartIntersection.NorthOutFlow();
            }

            if (this == StartIntersection.GetSouthLink())
            {
                return StartIntersection.SouthOutFlow();
            }
            return double.NaN;
        }

        public double EndInflow()
        {

            if (this == EndIntersection.GetEastLink())
            {
                return EndIntersection.EastInFlow();
            }

            if (this == EndIntersection.GetWestLink())
            {
                return EndIntersection.WestInFlow();
            }

            if (this == EndIntersection.GetNorthLink())
            {
                return EndIntersection.NorthInFlow();
            }

            if (this == EndIntersection.GetSouthLink())
            {
                return EndIntersection.SouthInFlow();
            }
            return double.NaN;
        }

        public double EndOutflow()
        {
            if (this == EndIntersection.GetEastLink())
            {
                return EndIntersection.EastOutFlow();
            }

            if (this == EndIntersection.GetWestLink())
            {
                return EndIntersection.WestOutFlow();
            }

            if (this == EndIntersection.GetNorthLink())
            {
                return EndIntersection.NorthOutFlow();
            }

            if (this == EndIntersection.GetSouthLink())
            {
                return EndIntersection.SouthOutFlow();
            }
            return double.NaN;
        }
        /*
        public bool isOutward(Intersection input)
        {
            if (input == this.StartIntersection)
            {
                return true;
            }
            return false;
        }

        public bool isInward(Intersection input)
        {
            if (input == this.EndIntersection)
            {
                return true;
            }
            return false;
        }*/
    }
}
