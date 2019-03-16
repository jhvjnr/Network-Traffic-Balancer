using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace WpfApp1
{
   public class Direction
    {
        public string Name { get; set; }
        public LineWithText LineWithText { get; set; }
        public double Flow { get; set; }

        public Direction(LineWithText inputLine, double flow)
        {
            Name = inputLine.Text.Text;
            LineWithText = inputLine;
            Flow = flow;
        }

        public Direction(LineWithText inputLine)
        {
            Name = inputLine.Text.Text;
            LineWithText = inputLine;
            Flow = 0;
        }
    }
}
