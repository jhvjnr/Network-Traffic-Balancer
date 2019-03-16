using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
   public class ApproachWithFan
    {
        public string Name { get; set; }
        public LineWithText LineWithText { get; set; }
        public List<Direction> Fan { get; set; }
        public List<Direction> SnappedFeathers { get; set; }
        public double Inflow { get; set; }
        public bool EndSnapped { get; set; }
        
        public double Outflow()
        {
            double sum = 0;
            foreach (Direction d in SnappedFeathers)
            {
                sum += d.Flow;
            }
            return sum;
        }

        public ApproachWithFan(LineWithText approachLine, List<Direction> fanLines, double inflow)
        {
            Name = approachLine.Text.Text;
            Inflow = inflow;
            EndSnapped = false;
            LineWithText = approachLine;
            Fan = fanLines;
            SnappedFeathers = new List<Direction>();
            LineWithText.Line.SizeChanged += (o, s) =>
                 {
                     var angleDiv = 180 / (Fan.Count + 1);
                     var currentAngle = LineWithText.OrientationDEG() + 90;
                     var directionLineAngle = -90;
                     var radius = 10;
                     foreach (Direction direction in Fan)
                     {
                         var fanLine = direction.LineWithText;
                         //radius = 10;
                         var startCoords = new Point(LineWithText.Line.X1 - Math.Cos((Math.PI / 180) * currentAngle) * 50 / 2, LineWithText.Line.Y1 + Math.Sin((Math.PI / 180) * currentAngle) * 50 / 2);
                         var rotAngle = (Math.PI / 180) * (directionLineAngle - currentAngle + angleDiv);
                         Point endCoords;

                         if (!fanLine.EndSnapped)
                         {
                             endCoords = new Point(fanLine.Line.X1 + radius * Math.Cos(rotAngle), fanLine.Line.Y1 + radius * Math.Sin(rotAngle));
                         }
                         else
                         {
                             endCoords = new Point(fanLine.Line.X2, fanLine.Line.Y2);
                         }

                         fanLine.Line.X1 = startCoords.X;
                         fanLine.Line.Y1 = startCoords.Y;
                         fanLine.Line.X2 = endCoords.X;
                         fanLine.Line.Y2 = endCoords.Y;

                         foreach (Direction snappedFeather in SnappedFeathers)
                         {
                             snappedFeather.LineWithText.Line.X2 = startCoords.X;
                             snappedFeather.LineWithText.Line.Y2 = startCoords.Y;
                         }
                         directionLineAngle += angleDiv;

                         //LineWithText currentDirectionLine = new LineWithText((Line)DrawLineOnCanvas(imageCanvas, startCoords, endCoords, brush, 4, 8), direction.Direction);
                     }
                 };
           
        }
    }
}
