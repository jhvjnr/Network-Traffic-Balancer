using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp1
{
   public class ApproachWithFan : UIElement
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public LineWithText LineWithText { get; set; }
        public LineWithText OutFlowIndicator { get; set; }
        public LineWithText InFlowIndicator { get; set; }
        public List<Direction> Fan { get; set; }
        public HashSet<Direction> SnappedFeathers { get; set; }
        public double Inflow { get; set; }
        public double Outflow
        {
            get
            {
                double sum = 0;
                foreach (Direction d in SnappedFeathers)
                {
                    sum += d.Flow;
                }
                return sum;
            }

            set
            {

            }
        }
        public bool EndSnapped { get; set; }
        public ApproachWithFan SnappedApproach { get; set; }

        public ApproachWithFan()
        {
            Fan = new List<Direction>();
            SnappedFeathers = new HashSet<Direction>();

            LineWithText.Line.SizeChanged += (o, s) =>
            {
                UpdateVisuals();
                LineWithText.Line.Stroke = new SolidColorBrush(MainWindow.ColorFromHSL(90 + LineWithText.OrientationDEG(), .5, .5));


                foreach (Direction feather in Fan)
                {
                    feather.LineWithText.Line.SizeChanged += (sender, args) =>
                    {
                        feather.LineWithText.Line.Stroke = new SolidColorBrush(MainWindow.ColorFromHSL(90 + LineWithText.OrientationDEG(), .5, .5));
                    };
                }

                //LineWithText currentDirectionLine = new LineWithText((Line)DrawLineOnCanvas(imageCanvas, startCoords, endCoords, brush, 4, 8), direction.Direction)
            };
        }

        public void DrawIndicators() // Draws the yellow / green bands that indicate the traffic flow volumes on the approach
        {
            var canvas = ((MainWindow)Application.Current.MainWindow).imageCanvas;
            if (OutFlowIndicator != null && canvas.Children.Contains(OutFlowIndicator.Line))
            {
                canvas.Children.Remove(OutFlowIndicator.Line);
                canvas.Children.Remove(OutFlowIndicator.Text);
            }

            if (InFlowIndicator != null && canvas.Children.Contains(InFlowIndicator.Line))
            {
                canvas.Children.Remove(InFlowIndicator.Line);
                canvas.Children.Remove(InFlowIndicator.Text);
            }

            Line OutFlowLine = new Line();

            OutFlowLine.Visibility = Visibility.Visible;

            OutFlowLine.Stroke = new SolidColorBrush(Color.FromArgb(150, 0, 254, 0));

            var thickness = 10 * Math.Log(Outflow / 2);

            if (double.IsNaN(thickness) || double.IsInfinity(thickness) || thickness < 0) thickness = 0;

            OutFlowLine.StrokeThickness = thickness;

            if (double.IsNaN(OutFlowLine.StrokeThickness)) OutFlowLine.StrokeThickness = 0;

            var dx = LineWithText.Line.X2 - LineWithText.Line.X1;

            var dy = LineWithText.Line.Y2 - LineWithText.Line.Y1;



            OutFlowLine.X2 = LineWithText.Line.X2 - thickness * Math.Cos(LineWithText.OrientationRAD()) / 2;

            OutFlowLine.Y2 = LineWithText.Line.Y2 + thickness * Math.Sin(LineWithText.OrientationRAD()) / 2;

            OutFlowLine.X1 = LineWithText.Line.X1 - thickness * Math.Cos(LineWithText.OrientationRAD()) / 2;

            OutFlowLine.Y1 = LineWithText.Line.Y1 + thickness * Math.Sin(LineWithText.OrientationRAD()) / 2;

            OutFlowIndicator = null;
            OutFlowIndicator = new LineWithText(OutFlowLine, "" + Outflow);

            canvas.Children.Add(OutFlowIndicator.Line);


            Line InflowLine = new Line();

            InflowLine.Visibility = Visibility.Visible;

            InflowLine.Stroke = new SolidColorBrush(Color.FromArgb(150, 254, 254, 0));

            var inthickness = 10 * Math.Log(Inflow / 2);

            if (double.IsNaN(inthickness) || double.IsInfinity(inthickness) || inthickness < 0) inthickness = 0;

            InflowLine.StrokeThickness = inthickness;

            if (double.IsNaN(InflowLine.StrokeThickness)) InflowLine.StrokeThickness = 0;

            dx = LineWithText.Line.X2 - LineWithText.Line.X1;

            dy = LineWithText.Line.Y2 - LineWithText.Line.Y1;



            InflowLine.X1 = LineWithText.Line.X1 + inthickness * Math.Cos(LineWithText.OrientationRAD()) / 2;

            InflowLine.Y1 = LineWithText.Line.Y1 - inthickness * Math.Sin(LineWithText.OrientationRAD()) / 2;

            InflowLine.X2 = LineWithText.Line.X2 + inthickness * Math.Cos(LineWithText.OrientationRAD()) / 2;

            InflowLine.Y2 = LineWithText.Line.Y2 - inthickness * Math.Sin(LineWithText.OrientationRAD()) / 2;

            InFlowIndicator = null;
            InFlowIndicator = new LineWithText(InflowLine, "" + this.Inflow);

            canvas.Children.Add(InflowLine);

            InFlowIndicator.Line.IsHitTestVisible = false;
            OutFlowIndicator.Line.IsHitTestVisible = false;
            OutFlowIndicator.Text.IsHitTestVisible = false;
            InFlowIndicator.Text.IsHitTestVisible = false;

            Panel.SetZIndex(InflowLine, -1);
            Panel.SetZIndex(OutFlowLine, -1);
        }

        public void UpdateVisuals()
        {
            DrawDirections();
            DrawIndicators();
            
            foreach (Direction dir in Fan)
            {
                dir.UpdateText();
            }
        }
        
        public void DrawDirections() // redraws directions in fan
        {
            var angleDiv = 180 / (Fan.Count + 1);
            var currentAngle = LineWithText.OrientationDEG() + 90;
            var directionLineAngle = -90;
            var radius = 10;
            foreach (Direction direction in Fan)
            {

                var fanLine = direction.GetLineWithText();
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
                    snappedFeather.GetLineWithText().Line.X2 = startCoords.X;
                    snappedFeather.GetLineWithText().Line.Y2 = startCoords.Y;
                }
                directionLineAngle += angleDiv;
            }
        }
        
        public ApproachWithFan(LineWithText approachLine, List<Direction> fanLines, double inflow)
        {
            Name = approachLine.Text.Text;
            Inflow = inflow;
            EndSnapped = false;
            LineWithText = approachLine;
            Fan = fanLines;
            SnappedFeathers = new HashSet<Direction>();
            LineWithText.Line.SizeChanged += (o, s) =>
                 {
                     UpdateVisuals();
                     LineWithText.Line.Stroke = new SolidColorBrush(MainWindow.ColorFromHSL(90 + LineWithText.OrientationDEG(), .5, .5));
                    
                     
                     foreach(Direction feather in Fan)
                     {
                         feather.LineWithText.Line.SizeChanged += (sender, args) =>
                         {
                             feather.LineWithText.Line.Stroke = new SolidColorBrush(MainWindow.ColorFromHSL(90 + LineWithText.OrientationDEG(), .5, .5));
                         };
                     }
                     
                     //LineWithText currentDirectionLine = new LineWithText((Line)DrawLineOnCanvas(imageCanvas, startCoords, endCoords, brush, 4, 8), direction.Direction)
                 };

            var frmMain = ((MainWindow)Application.Current.MainWindow);
            this.LineWithText.Line.MouseDown += (o, e) =>
            {
                Point mousePosition = e.GetPosition(frmMain);
                frmMain.Title = "Mouse down on Line " + mousePosition.X;
                frmMain.initialMousePosition = mousePosition;
                frmMain.previousMousePosition = frmMain.initialMousePosition;
                //initialLineEndPosition = new Point(currentApproachLine.Line.X2, currentApproachLine.Line.Y2);
                //frmMain.isMouseDown = true;
                frmMain.toMove = (UIElement)o;

                //GetDirectionFromLine((Line)o).LineWithText.EndSnapped = false;
                frmMain.DeSnapLine((Line)o);
                frmMain.DeSnapApproach(this);
            };

            frmMain.MouseMove += (o, s) =>
            {
                if (s.LeftButton == System.Windows.Input.MouseButtonState.Pressed && frmMain.toMove != null && !this.EndSnapped)
                {
                    // var snapLine = GetDirectionFromLine(Intersections[toAdd.Name], (Line)toMove);
                    //if (snapLine != null) SnapFeather(Intersections[toAdd.Name], snapLine);
                    var toSnapApproach = frmMain.GetApproachWithFanFromLine((Line)frmMain.toMove);
                    if (toSnapApproach != null) frmMain.SnapApproach(toSnapApproach);
                }

            };


        }

        
    }
}
