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

    public class LineWithText
    {

       
        public Line Line { get; set; }
        public TextBlock Text { get; set; }
        public bool EndSnapped { get; set; }


        public LineWithText(Line inputLine, string text)
        {

            EndSnapped = false;
            Line = inputLine;
            TextBlock TextTemp = new TextBlock
            {
                Text = text,
                Foreground = Brushes.Black,
                RenderTransformOrigin = new Point(0.5, 0.5),
                IsHitTestVisible = false,
                FontSize = Math.Max(Line.StrokeThickness - 1, 1),
            };

            this.Text = TextTemp;


            Line.SizeChanged += (sender, args) => // ensures line text moves with line
            {
                double rotationAngle = (180 / Math.PI) * Math.Atan((Line.Y2 - Line.Y1) / (Line.X2 - Line.X1));
                if (Line.X2 - Line.X1 == 0) rotationAngle = 0;

                var lineRotation = new RotateTransform
                {
                    Angle = rotationAngle,
                    CenterX = (Line.X1 + Line.X2) / 2,
                    CenterY = (Line.Y1 + Line.Y2) / 2
                };


                Text.LayoutTransform = lineRotation;
                var canvas = LogicalTreeHelper.GetParent(Line);
                if (!((Canvas)(canvas)).IsAncestorOf(Text)) ((Canvas)(canvas)).Children.Add(Text);
                ((Canvas)(canvas)).UpdateLayout();

                var left = (Line.X1 + Line.X2) / 2 - Text.DesiredSize.Width / 2;
                var top = (Line.Y1 + Line.Y2) / 2 + -.5 * Text.DesiredSize.Height;

                Canvas.SetLeft(Text, left);
                Canvas.SetTop(Text, top);

                Panel.SetZIndex(Text, Panel.GetZIndex(Line) + 1);
            };
            this.Line.Height += 1;


        }



        public double OrientationDEG()
        {
            var dx = Line.X2 - Line.X1;
            var dy = Line.Y2 - Line.Y1;

            return (180 / Math.PI) * Math.Atan2(dx, dy);
        }

        public double OrientationRAD()
        {
            var dx = Line.X2 - Line.X1;
            var dy = Line.Y2 - Line.Y1;

            return Math.Atan2(dx, dy);
        }
    }
}
