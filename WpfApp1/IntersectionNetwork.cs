using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;

namespace WpfApp1
{
   public class IntersectionNetwork
    {
        private Dictionary<string, Intersection> _intersections;
        public Dictionary<string, Intersection> Intersections
        {
            get
            {
                return _intersections;
            }
        }

        public IntersectionNetwork()
        {
            _intersections = new Dictionary<string, Intersection>();
        }

        public void compileIDs()
        {
            foreach (Intersection intersection in _intersections.Values)
            {
                intersection.ID = intersection.Name;
                foreach (ApproachWithFan approach in intersection.Approaches)
                {
                    approach.ID = intersection.ID + approach.Name;
                    foreach (Direction direction in approach.Fan)
                    {
                        direction.ID = approach.ID + direction.Name;
                    }
                }
            }
        }

        public void WriteToFile(UIElement canvas, string filename)
        {
            compileIDs();
            XmlDocument doc = new XmlDocument();

            XmlNode networkRoot = doc.CreateElement("IntersectionNetwork");
            doc.AppendChild(networkRoot);

            foreach (Intersection intersection in _intersections.Values)
            {
                XmlNode interNode = doc.CreateElement("Intersection");
                networkRoot.AppendChild(interNode);
                
                XmlNode intIDNode = doc.CreateElement("ID");
                intIDNode.InnerText = intersection.ID;
                interNode.AppendChild(intIDNode);

                XmlNode intNameNode = doc.CreateElement("Name");
                intNameNode.InnerText = intersection.Name;
                interNode.AppendChild(intNameNode);

                XmlNode intXCoord = doc.CreateElement("X");
                intXCoord.InnerText = "" + intersection.Ellipse.TranslatePoint(new Point(0, 0), canvas).X;
                interNode.AppendChild(intXCoord);

                XmlNode intYCoord = doc.CreateElement("Y");
                intYCoord.InnerText = "" + intersection.Ellipse.TranslatePoint(new Point(0, 0), canvas).Y;
                interNode.AppendChild(intYCoord);


                XmlNode intApproaches = doc.CreateElement("Approaches");

                foreach (ApproachWithFan approach in intersection.Approaches)
                {
                    XmlNode apprNode = doc.CreateElement("ApproachWithFan");
                    intApproaches.AppendChild(apprNode);

                    XmlNode apprID = doc.CreateElement("ID");
                    apprID.InnerText = approach.ID;
                    apprNode.AppendChild(apprID);

                    XmlNode apprName = doc.CreateElement("Name");
                    apprName.InnerText = approach.Name;
                    apprNode.AppendChild(apprName);


                    XmlNode apprLineWithText = doc.CreateElement("LineWithText");

                    XmlNode apprLineX1 = doc.CreateElement("X1");
                    apprLineX1.InnerText = "" + approach.LineWithText.Line.X1;
                    apprLineWithText.AppendChild(apprLineX1);

                    XmlNode apprLineY1 = doc.CreateElement("Y1");
                    apprLineY1.InnerText = "" + approach.LineWithText.Line.Y1;
                    apprLineWithText.AppendChild(apprLineY1);

                    XmlNode apprLineX2 = doc.CreateElement("X2");
                    apprLineX2.InnerText = "" + approach.LineWithText.Line.X2;
                    apprLineWithText.AppendChild(apprLineX2);

                    XmlNode apprLineY2 = doc.CreateElement("Y2");
                    apprLineY2.InnerText = "" + approach.LineWithText.Line.Y2;
                    apprLineWithText.AppendChild(apprLineY2);

                    XmlNode apprLineText = doc.CreateElement("Text");
                    apprLineText.InnerText = approach.LineWithText.Text.Text;
                    apprLineWithText.AppendChild(apprLineText);

                    XmlNode apprLineEndSnapped = doc.CreateElement("EndSnapped");
                    apprLineEndSnapped.InnerText = "" + approach.LineWithText.EndSnapped;
                    apprLineWithText.AppendChild(apprLineEndSnapped);

                    apprNode.AppendChild(apprLineWithText);



                    XmlNode apprFeathers = doc.CreateElement("Feathers");
                    apprNode.AppendChild(apprFeathers);

                    foreach (Direction direction in approach.Fan)
                    {
                        XmlNode dirNode = doc.CreateElement("Direction");
                        apprFeathers.AppendChild(dirNode);

                        XmlNode dirID = doc.CreateElement("ID");
                        dirID.InnerText = direction.ID;
                        dirNode.AppendChild(dirID);

                        XmlNode dirName = doc.CreateElement("Name");
                        dirName.InnerText = direction.Name;
                        dirNode.AppendChild(dirName);

                        XmlNode dirLineWithText = doc.CreateElement("LineWithText");

                        XmlNode dirLineX1 = doc.CreateElement("X1");
                        dirLineX1.InnerText = "" + direction.LineWithText.Line.X1;
                        dirLineWithText.AppendChild(dirLineX1);

                        XmlNode dirLineY1 = doc.CreateElement("Y1");
                        dirLineY1.InnerText = "" + direction.LineWithText.Line.Y1;
                        dirLineWithText.AppendChild(dirLineY1);

                        XmlNode dirLineX2 = doc.CreateElement("X2");
                        dirLineX2.InnerText = "" + direction.LineWithText.Line.X2;
                        dirLineWithText.AppendChild(dirLineX2);

                        XmlNode dirLineY2 = doc.CreateElement("Y2");
                        dirLineY2.InnerText = "" + direction.LineWithText.Line.Y2;
                        dirLineWithText.AppendChild(dirLineY2);

                        XmlNode dirLineText = doc.CreateElement("Text");
                        dirLineText.InnerText = direction.LineWithText.Text.Text;
                        dirLineWithText.AppendChild(dirLineText);

                        XmlNode dirLineEndSnapped = doc.CreateElement("EndSnapped");
                        dirLineEndSnapped.InnerText = "" + direction.LineWithText.EndSnapped;
                        dirLineWithText.AppendChild(dirLineEndSnapped);

                        dirNode.AppendChild(dirLineWithText);



                    }

                    XmlNode apprSnappedFeathers = doc.CreateElement("SnappedFeathers");
                    apprNode.AppendChild(apprSnappedFeathers);

                    foreach (Direction snappedFeather in approach.SnappedFeathers)
                    {
                        XmlNode snappedFeatherNode = doc.CreateElement("Direction");
                        apprSnappedFeathers.AppendChild(snappedFeatherNode);

                        XmlNode dirID = doc.CreateElement("ID");
                        dirID.InnerText = snappedFeather.ID;
                        snappedFeatherNode.AppendChild(dirID);
                    }

                    XmlNode apprEndSnapped = doc.CreateElement("EndSnapped");
                    apprEndSnapped.InnerText = "" + approach.EndSnapped;
                    apprNode.AppendChild(apprEndSnapped);

                    XmlNode apprSnappedApproach = doc.CreateElement("SnappedApproach");
                    if (approach.SnappedApproach != null)
                    {
                        apprSnappedApproach.InnerText = approach.SnappedApproach.ID;
                    }
                    else
                    {
                        apprSnappedApproach.InnerText = "";
                    }
                    apprNode.AppendChild(apprSnappedApproach);
                }
                interNode.AppendChild(intApproaches);
            }
            if (filename.Contains(".xml"))
            {
                doc.Save(filename);
            }
            else
            {
                doc.Save(filename + ".xml");
            }
        }

        public static IntersectionNetwork LoadFromFile(string fileName, Canvas canvas, LinkedList<FlatFileRecord> records, MainWindow window)
        {
            IntersectionNetwork output = new IntersectionNetwork();
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            foreach (XmlNode intersectionNode in doc.ChildNodes[0])
            {
                string intName = intersectionNode["Name"].InnerText;
                Ellipse intEllipse = new Ellipse
                {
                    Stroke = Brushes.Blue,
                    Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255)),
                    Width = 50,
                    Height = 50
                };

                Canvas.SetLeft(intEllipse, double.Parse(intersectionNode["X"].InnerText));
                Canvas.SetTop(intEllipse, double.Parse(intersectionNode["Y"].InnerText));
                canvas.UpdateLayout();
                Panel.SetZIndex(intEllipse, 7);
                canvas.Children.Add(intEllipse);

                Intersection currentIntersection = new Intersection(intName, intEllipse);
                foreach (XmlNode approachNode in intersectionNode["Approaches"].ChildNodes)
                {
                    
                    double inFlow = records.Where(x => x.Approach == approachNode["Name"].InnerText && x.CommuterClass == (string)window.cbxClasses.SelectedItem && x.DateTime.TimeOfDay == (TimeSpan)window.cbxTimes.SelectedValue && x.IntersectionName == intersectionNode["Name"].InnerText).Select(x => x.Count).Sum();


                    XmlNode lineWithTextNode = approachNode["LineWithText"];

                    Point startCoords = new Point(double.Parse(lineWithTextNode["X1"].InnerText), double.Parse(lineWithTextNode["Y1"].InnerText));
                    Point endCoords = new Point(double.Parse(lineWithTextNode["X2"].InnerText), double.Parse(lineWithTextNode["Y2"].InnerText));
                    double angle = 180 * Math.Atan2(endCoords.Y - startCoords.Y, endCoords.X - startCoords.X) / Math.PI;
                    Brush brush = new SolidColorBrush(MainWindow.ColorFromHSL(angle, .5, .5));
                    LineWithText currentApproachLine = new LineWithText((Line)MainWindow.DrawLineOnCanvas(canvas, startCoords, endCoords, brush, 14, 0), lineWithTextNode["Text"].InnerText);
                    currentApproachLine.EndSnapped = bool.Parse(lineWithTextNode["EndSnapped"].InnerText);

                    List<Direction> fanLines = new List<Direction>();
                    foreach (XmlNode directionNode in approachNode["Feathers"].ChildNodes)
                    {


                        XmlNode dirlineWithTextNode = directionNode["LineWithText"];
                        
                        Point dirstartCoords = new Point(double.Parse(dirlineWithTextNode["X1"].InnerText), double.Parse(dirlineWithTextNode["Y1"].InnerText));
                        Point direndCoords = new Point(double.Parse(dirlineWithTextNode["X2"].InnerText), double.Parse(dirlineWithTextNode["Y2"].InnerText));
                        brush = new SolidColorBrush(MainWindow.ColorFromHSL(-angle + 180, .5, .5));
                        LineWithText currentDirectionLine = new LineWithText((Line)MainWindow.DrawLineOnCanvas(canvas, dirstartCoords, direndCoords, brush, 4, 8), directionNode["Name"].InnerText);
                        
                        currentDirectionLine.EndSnapped = bool.Parse(dirlineWithTextNode["EndSnapped"].InnerText);

                   
                        FlatFileRecord record = records.Single(x => x.IntersectionName == currentIntersection.Name && x.CommuterClass == (string)window.cbxClasses.SelectedItem && x.DateTime.TimeOfDay == (TimeSpan)window.cbxTimes.SelectedValue && x.Direction == directionNode["Name"].InnerText && x.Approach == approachNode["Name"].InnerText);
                        int dirFlow = record.Count;
                       // LineWithText dirLine = new LineWithText((Line)MainWindow.DrawLineOnCanvas(canvas, startCoords, endCoords, brush, 4, 8), directionNode["Name"].InnerText);
                        
                        Direction currentDirection = new Direction(currentDirectionLine, dirFlow, record);
                        fanLines.Add(currentDirection);
                    }
                    ApproachWithFan currentApproach = new ApproachWithFan(currentApproachLine, fanLines, inFlow);
                    currentIntersection.Approaches.Add(currentApproach);
                    foreach (XmlNode snappedFeatherNode in approachNode["SnappedFeathers"].ChildNodes)
                    {

                    }

                 
                }
                
                output.Intersections.Add(currentIntersection.Name, currentIntersection);

            }


            return output;
        }

        private UIElement DrawLineOnCanvas(Canvas canvas, Point startCoords, Point endCoords, Brush brush, double thickness, int renderpriority)
        {
            var line = new Line
            {
                Visibility = Visibility.Visible,
                StrokeThickness = thickness,
                Stroke = brush,
                X1 = startCoords.X,
                Y1 = startCoords.Y,
                X2 = endCoords.X,
                Y2 = endCoords.Y
            };

            double rotationAngle = (180 / Math.PI) * Math.Atan((line.Y2 - line.Y1) / (line.X2 - line.X1));
            if (line.X2 - line.X1 == 0) rotationAngle = 0;

            Panel.SetZIndex(line, renderpriority);
            canvas.Children.Add(line);
            return line;
        }

            public void registerEllipse(Ellipse intEllipse, MainWindow window)
        {

        }
    }
}
