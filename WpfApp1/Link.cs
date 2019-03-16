using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace WpfApp1
{
    public class Link
    {
        public Intersection StartIntersection { get; set; }
        public Intersection EndIntersection { get; set; }
        public Line Line { get; set; }
        public double InFlow { get; set; }
        public double OutFlow { get; set; }
        public string Name { get; set; }

        public double orientation()
        {
            var dy = Line.Y2 - Line.Y1;
            var dx = Line.X2 - Line.X1;

            if (dx == 0) return 0;
            return Math.Atan(dy / dx);
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
    }
}
