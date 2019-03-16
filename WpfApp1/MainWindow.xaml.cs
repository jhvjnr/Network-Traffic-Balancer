using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using MapControl;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point initialMousePosition;
        private Point previousMousePosition;
        private Point initialImagePosition;
        private UIElement toMove;
        private bool isMouseDown = false;
        private int clickNum = 0;
        private Point lineStart;
        private Intersection startIntersection;
        private LinkedList<FlatFileRecord> records = new LinkedList<FlatFileRecord>();
        private RoadNetwork network = new RoadNetwork();
        private List<UIElement> renderList = new List<UIElement>();
        private List<ApproachWithFan> Approaches = new List<ApproachWithFan>();
        private double zoomFactor = 1;

        public MainWindow()
        {
            TileGenerator.CacheFolder = @"TempCache";
            InitializeComponent();
        }

        private void imgMap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePosition = e.GetPosition(this);
            
            frmMain.Title = "" + mousePosition.X;
            this.initialMousePosition = mousePosition;
            previousMousePosition = initialMousePosition;
            initialImagePosition = new Point(Canvas.GetLeft(imageCanvas), Canvas.GetTop(imageCanvas));
            this.isMouseDown = true;
        }

        private void imgMap_MouseMove(object sender, MouseEventArgs e)
        {
            frmMain.Title = "mouse is moving over image";
            if (isMouseDown && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Point deltaPos = new Point(e.GetPosition(this).X - previousMousePosition.X, e.GetPosition(this).Y - previousMousePosition.Y);
                /* frmMain.Title = "Trying to move image";
                 Point newPos = new Point(e.GetPosition(this).X - initialMousePosition.X, e.GetPosition(this).Y - initialMousePosition.Y);
                 imageCanvas.RenderTransform.Transform(newPos);
                 Canvas.SetLeft(imageCanvas, initialImagePosition.X + newPos.X);
                 Canvas.SetTop(imageCanvas, initialImagePosition.Y + newPos.Y);*/

                var layoutMatrix = imageCanvas.RenderTransform as MatrixTransform;
                var matrix = layoutMatrix.Matrix;
                matrix.Translate(deltaPos.X, deltaPos.Y);
                layoutMatrix.Matrix = matrix;
                previousMousePosition = e.GetPosition(this);
                
            }
        }

        private void imgMap_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
        }

        private void imgMap_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            frmMain.Title = "" + e.Delta;

            var layoutMatrix = imageCanvas.RenderTransform as MatrixTransform;
            var matrix = layoutMatrix.Matrix;
            zoomFactor = Math.Sign(e.Delta) * .05 + 1;

            matrix.ScaleAtPrepend(zoomFactor, zoomFactor, e.GetPosition(imageCanvas).X, e.GetPosition(imageCanvas).Y);
            layoutMatrix.Matrix = matrix;
        }

        private void frmMain_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
        }

        private void imgMap_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            frmMain.Title = "Trying to place Circle";

            var myCircle = new Ellipse
            {
                Stroke = Brushes.Blue,
                Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255)),
                Width = 50,
                Height = 50
            };





            Canvas.SetLeft(myCircle, e.GetPosition(imageCanvas).X - myCircle.Width / 2);
            Canvas.SetTop(myCircle, e.GetPosition(imageCanvas).Y - myCircle.Height / 2);
            imageCanvas.UpdateLayout();
            Panel.SetZIndex(myCircle, 7);
            imageCanvas.Children.Add(myCircle);

            var toAdd = new Intersection((string)lstOut.SelectedValue, myCircle);
            network.intersections.Add(toAdd);

            TextBlock myCircleText = new TextBlock
            {
                Text = network.intersections.Last().Name,
                Foreground = Brushes.White,
                FontSize = 20,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(myCircleText, e.GetPosition(imageCanvas).X + myCircle.Width / 2);
            Canvas.SetTop(myCircleText, e.GetPosition(imageCanvas).Y - myCircle.Height / 2);
            Panel.SetZIndex(myCircleText, 2);
            imageCanvas.Children.Add(myCircleText);
            UpdateLayout();
            lstOut.SelectedIndex++;
            lstOut.Focus();
            if (lstOut.Items.Count > 0) frmMain.Title = network.intersections.Last().Name;



            var links = records.Where(x => x.IntersectionName == toAdd.Name && x.CommuterClass == (string)cbxClasses.SelectedItem && x.DateTime.TimeOfDay == (TimeSpan)cbxTimes.SelectedValue);//.GroupBy(x => new { x.Direction, x.Approach });
            if (links.Count() == 0) return;
            var approaches = links.GroupBy(x => x.Approach).Select(g => g.First());
            double divAngle = 360 / approaches.Count();
            double currentAngle = 90;


            
            foreach (var approach in approaches)
            {
               // var fans = new LinkedList<ApproachWithFan>();
                double radius = 100;
                var startCoords = myCircle.TranslatePoint(new Point(0, 0), imageCanvas);
                startCoords.X += myCircle.Width / 2;
                startCoords.Y += myCircle.Height / 2;

                var endCoords = new Point( startCoords.X + radius * Math.Cos((Math.PI / 180) * currentAngle), startCoords.Y + radius * Math.Sin((Math.PI / 180) * currentAngle));

                Brush brush = new SolidColorBrush(ColorFromHSL(currentAngle, .5, .5));
                

                LineWithText currentApproachLine = new LineWithText((Line) DrawLineOnCanvas(imageCanvas, startCoords, endCoords, brush, 14,0), approach.Approach);

                
                currentApproachLine.Line.MouseDown += mouseDownToMoveLine;
                
                /*currentApproachLine.Text.MouseDown += (senderOf, args) =>
                {
                    Point mousePosition = e.GetPosition(this);
                    frmMain.Title = "" + mousePosition.X;
                    this.initialMousePosition = mousePosition;
                    previousMousePosition = initialMousePosition;
                    //initialLineEndPosition = new Point(currentApproachLine.Line.X2, currentApproachLine.Line.Y2);
                    this.isMouseDown = true;
                    this.toMove = currentApproachLine.Line;

                };*/

                /*(senderOf, args) =>
                {
                    Point mousePosition = e.GetPosition(this);
                    frmMain.Title = "" + mousePosition.X;
                    this.initialMousePosition = mousePosition;
                    previousMousePosition = initialMousePosition;
                    //initialLineEndPosition = new Point(currentApproachLine.Line.X2, currentApproachLine.Line.Y2);
                    this.isMouseDown = true;
                    this.toMove = currentApproachLine.Line;
                    
                };*/
                //                currentApproachLine.Text.MouseDown += currentApproachLine.Line.MouseDown();
                frmMain.MouseMove += (senderOf, args) =>
                {

                    if (isMouseDown && toMove != null)
                    {
                        var lineToMove = (Line)toMove;
                        //Point deltaPos = new Point(e.GetPosition(this).X - previousMousePosition.X, e.GetPosition(this).Y - previousMousePosition.Y);
                        double dx = e.GetPosition(imageCanvas).X - lineToMove.X1;
                        double dy = e.GetPosition(imageCanvas).Y - lineToMove.Y1;
                        double angle = Math.Atan2(dx, dy);
                        lineToMove.X2 = e.GetPosition(imageCanvas).X + Math.Sin(angle) * lineToMove.StrokeThickness / 2;//+= deltaPos.X / zoomFactor;
                        lineToMove.Y2 = e.GetPosition(imageCanvas).Y +  Math.Cos(angle) * lineToMove.StrokeThickness / 2; //+= deltaPos.Y / zoomFactor;
                        previousMousePosition = e.GetPosition(this);
                    }
                };

                var directions = links.Where(x => x.Approach == approach.Approach).GroupBy(x => x.Direction).Select(g => g.First());

                double directionLineAngle = 0;
                double directAngleDiv = 180 / (directions.Count() + 1);

                var fanLines = new List<Direction>();

                foreach (var direction in directions)
                {
                    radius = 10;
                    startCoords = new Point(currentApproachLine.Line.X1 + Math.Cos((Math.PI / 180) * currentAngle) * myCircle.Width / 2, currentApproachLine.Line.Y1 + Math.Sin((Math.PI / 180) * currentAngle) * myCircle.Height / 2);
                    var rotAngle = (Math.PI / 180) * (directionLineAngle + currentAngle + directAngleDiv + 90);
                    endCoords = new Point(startCoords.X + radius * Math.Cos(rotAngle), startCoords.Y + radius * Math.Sin(rotAngle));


                    directionLineAngle += directAngleDiv;


                    Direction currentDirectionLine = new Direction(new LineWithText((Line)DrawLineOnCanvas(imageCanvas, startCoords, endCoords, brush, 4, 8), direction.Direction), direction.Count);
                    currentDirectionLine.LineWithText.Text.Text = currentDirectionLine.Name + ": " + currentDirectionLine.Flow;
                    fanLines.Add(currentDirectionLine);
                    currentDirectionLine.LineWithText.Line.MouseDown += mouseDownToMoveLine;

                    /* currentDirectionLine.Text.MouseDown += (senderOf, args) =>
                     {
                         Point mousePosition = e.GetPosition(this);
                         frmMain.Title = "" + mousePosition.X;
                         this.initialMousePosition = mousePosition;
                         previousMousePosition = initialMousePosition;
                         this.isMouseDown = true;
                         this.toMove = currentDirectionLine.Line;

                     };*/

                    frmMain.MouseMove+= (o, s) =>
                    {
                        if (isMouseDown && toMove != null && !currentDirectionLine.LineWithText.EndSnapped)
                        {
                            var snapLine = GetDirectionFromLine((Line)toMove);
                            if (snapLine != null) SnapFeather(snapLine);
                        }
                    };

                   // currentDirectionLine.Line.MouseUp 

                    frmMain.MouseMove += (senderOf, args) =>
                    {
                        if (isMouseDown && toMove != null)
                        {
                            
                            Point deltaPos = new Point(e.GetPosition(this).X - previousMousePosition.X, e.GetPosition(this).Y - previousMousePosition.Y);
                            ((Line)(toMove)).X2 += deltaPos.X;
                            ((Line)(toMove)).Y2 += deltaPos.Y;
                            previousMousePosition = e.GetPosition(this);
                        }
                    };
                    
                }
                double approachInflow = links.Where(x => x.Approach == approach.Approach && x.IntersectionName == approach.IntersectionName).Select(x => x.Count).Sum();
                var newApproachWithFan = new ApproachWithFan(currentApproachLine, fanLines, approachInflow);
                newApproachWithFan.LineWithText.Text.Text = newApproachWithFan.Name + ": " + newApproachWithFan.Inflow;
                Approaches.Add(newApproachWithFan);
                
                currentAngle += divAngle;
            }
            
            myCircle.MouseRightButtonUp += (object circle, MouseButtonEventArgs mouseEvent) =>
            {
                ++clickNum;
                frmMain.Title = "Delegate function hooray!";

                switch (clickNum)
                {
                    case 1:
                        {
                            lineStart = myCircle.TranslatePoint(new Point(0, 0), imageCanvas);
                            frmMain.Title = lineStart.X + "";
                            startIntersection = network.intersections.Single(x => x.Ellipse == myCircle);
                        }
                        break;
                    case 2:
                        {
                            var lineEnd = myCircle.TranslatePoint(new Point(0, 0), imageCanvas);
                            frmMain.Title = lineEnd.X + "";

                            var line = new Line
                            {
                                Visibility = Visibility.Visible,
                                StrokeThickness = 10,
                                Stroke = Brushes.Red,
                                X1 = lineStart.X + myCircle.Width / 2,
                                Y1 = lineStart.Y + myCircle.Height / 2,
                                X2 = lineEnd.X + myCircle.Width / 2,
                                Y2 = lineEnd.Y + myCircle.Height / 2
                            };

                            double rotationAngle = (180 / Math.PI) * Math.Atan((line.Y2 - line.Y1) / (line.X2 - line.X1));
                            if (line.X2 - line.X1 == 0) rotationAngle = 0;

                            Panel.SetZIndex(line, 0);
                            imageCanvas.Children.Add(line);

                            Intersection end = network.intersections.Single(x => x.Ellipse == myCircle);
                            network.links.Add(new Link(startIntersection, end, line));
                            startIntersection.Links.Add(network.links.Last());
                            end.Links.Add(network.links.Last());


                            string roadName = "No common road name";
                            if (end.Name != null && startIntersection.Name != null)
                            {

                                string[] startNames = startIntersection.Name.Replace(" ", string.Empty).Split('-');
                                string[] endNames = end.Name.Split('-');


                                foreach (var startName in startNames)
                                {
                                    if (end.Name.Replace(" ", string.Empty).Contains(startName))
                                    {
                                        roadName = startName;
                                    }
                                }
                            }
                            TextBlock myEdgeText = new TextBlock
                            {
                                Text = roadName, // network.links.Last().StartIntersection.Name + "-->>" + network.links.Last().EndIntersection.Name,
                                Foreground = Brushes.Black,
                                RenderTransformOrigin = new Point(0.5, 0.5),
                                FontSize = 8

                            };


                            var lineRotation = new RotateTransform
                            {
                                Angle = rotationAngle,
                                CenterX = (line.X1 + line.X2) / 2,
                                CenterY = (line.Y1 + line.Y2) / 2
                            };


                            myEdgeText.LayoutTransform = lineRotation;
                            imageCanvas.Children.Add(myEdgeText);
                            UpdateLayout();

                            if (rotationAngle > 0)
                            {
                                var left = (line.X1 + line.X2) / 2 - myEdgeText.DesiredSize.Width / 2;
                                var top = (line.Y1 + line.Y2) / 2 + -.5 * myEdgeText.DesiredSize.Height;

                                Canvas.SetLeft(myEdgeText, left);
                                Canvas.SetTop(myEdgeText, top);
                            }
                            else
                            {
                                var left = (line.X1 + line.X2) / 2 - myEdgeText.DesiredSize.Width / 2;
                                var top = (line.Y1 + line.Y2) / 2 + -.5 * myEdgeText.DesiredSize.Height;

                                Canvas.SetLeft(myEdgeText, left);
                                Canvas.SetTop(myEdgeText, top);
                            }

                            Panel.SetZIndex(myEdgeText, 2);

                            frmMain.Title = myEdgeText.ActualHeight + ", " + myEdgeText.DesiredSize.Height + "," + myEdgeText.Height;
                            clickNum = 0;

                            // network.saveNetworkAsXML();
                            //network.saveNetworkAsBinary();
                        }
                        break;
                    default:
                        {

                        }
                        break;
                }
            };
        }

        private static double lineLength(Line line)
        {
            var dx = line.X2 - line.X1;
            var dy = line.Y2 - line.Y1;

            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void btnExitClick(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnLoadIntersectionClick(object sender, EventArgs e)
        {


            var opn = new Microsoft.Win32.OpenFileDialog();
            var result = opn.ShowDialog();

            if (result == true)
            {
                frmMain.Title = opn.FileName;
                var reader = new StreamReader(opn.FileName);
                if (!reader.EndOfStream)
                {
                    var fields = reader.ReadLine();
                }
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    string intersectionName = values[1];
                    string[] timeStrings = values[6].Split(':');
                    TimeSpan time = new TimeSpan(int.Parse(timeStrings[0]), int.Parse(timeStrings[1]), 0);

                    DateTime dateTime;
                    bool dateSuccess = DateTime.TryParse(values[2].Split(' ')[0], out dateTime);//values[2].Split(' ')[0]).Add(time);
                    if (dateTime != null) dateTime = dateTime.Add(time);
                    string approach = values[3];
                    string direction = values[4];
                    string comClass = values[5];
                    int count = 0;
                    int.TryParse(values[7], out count);

                    if (dateSuccess) records.AddLast(new FlatFileRecord(intersectionName, dateTime, approach, direction, comClass, count));
                }
                var intersectionsDistinct = records.GroupBy(x => x.IntersectionName).Select(g => g.First()).Select(x => x.IntersectionName);
                lstOut.ItemsSource = intersectionsDistinct;

                var times = records.GroupBy(x => x.DateTime.TimeOfDay).Select(g => g.First()).Select(p => p.DateTime.TimeOfDay).ToList();
                cbxTimes.ItemsSource = times;

                var classes = records.GroupBy(x => x.CommuterClass).Select(g => g.First()).Select(p => p.CommuterClass).ToList();
                cbxClasses.ItemsSource = classes;

                cbxClasses.SelectedIndex = 0;
                cbxTimes.SelectedIndex = 0;
                lstOut.SelectedIndex = 0;
            }
        }

        private void loadCounts()
        {
            foreach (var element in renderList)
            {
                imageCanvas.Children.Remove(element);
            }

            renderList = new List<UIElement>();

            
                


                UpdateLayout();
               
        }


        private Intersection GetIntersectionFromCircle(UIElement circle)
        {
            return network.intersections.Single(x => x.Ellipse == circle);
        }

        private void btnAssignIntersections(object sender, EventArgs e)
        {

        }

        private void BtnAddNumbers(object sender, EventArgs e)
        {
            loadCounts();
        }

        private void cbxTimes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadCounts();
        }

        private void cbxClasses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadCounts();
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
            /*
            TextBlock myEdgeText = new TextBlock
            {
                Text = text,
                Foreground = Brushes.Black,
                RenderTransformOrigin = new Point(0.5, 0.5),
                FontSize = thickness - 2

            };


            var lineRotation = new RotateTransform
            {
                Angle = rotationAngle,
                CenterX = (line.X1 + line.X2) / 2,
                CenterY = (line.Y1 + line.Y2) / 2
            };


            myEdgeText.LayoutTransform = lineRotation;
            canvas.Children.Add(myEdgeText);
            UpdateLayout();

            var left = (line.X1 + line.X2) / 2 - myEdgeText.DesiredSize.Width / 2;
            var top = (line.Y1 + line.Y2) / 2 + -.5 * myEdgeText.DesiredSize.Height;

            Canvas.SetLeft(myEdgeText, left);
            Canvas.SetTop(myEdgeText, top);

            Panel.SetZIndex(myEdgeText, 2);*/

            return line;
        }

        Color ColorFromHSL(double H, double S, double L)
        {
            S = Math.Min(S, 1);
            L = Math.Min(L, 1);
            S = Math.Max(S, 0);
            L = Math.Max(L, 0);

            H = H % 360;
            if (H < 0) H += 360;

            var chroma = (1 - Math.Abs(2 * L - 1)) * S * 255;
            var HPrime = H / 60;

            var X = chroma * (1 - Math.Abs(HPrime % 2 - 1));
            byte R1 = 0;
            byte G1 = 0;
            byte B1 = 0;

            if (HPrime >= 0 && HPrime < 1)
            {
                R1 = (byte) chroma;
                G1 = (byte) X;
                B1 = 0;
            }

            if (HPrime >= 1 && HPrime < 2)
            {
                R1 = (byte)X;
                G1 = (byte)chroma;
                B1 = 0;
            }

            if (HPrime >= 2 && HPrime < 3)
            {
                R1 = 0;
                G1 = (byte)chroma;
                B1 = (byte)X;
            }

            if (HPrime >= 3 && HPrime < 4)
            {
                R1 = 0;
                G1 = (byte)X;
                B1 = (byte)chroma;
            }

            if (HPrime >= 4 && HPrime < 5)
            {
                R1 = (byte)X;
                G1 = 0;
                B1 = (byte)chroma;
            }

            if (HPrime >= 5 && HPrime < 6)
            {
                R1 = (byte)chroma;
                G1 = 0;
                B1 = (byte)X;
            }

            byte m = (byte)( (L - chroma / 2) * 255) ;
            byte R = (byte)( R1 + m);
            byte G = (byte)(G1 + m);
            byte B = (byte)(B1 + m);

            return Color.FromArgb(255, R, G, B);
            //throw new NotImplementedException();
        }

        public void mouseDownToMoveLine(object o, MouseEventArgs args)
        {
            Point mousePosition = args.GetPosition(this);
            frmMain.Title = "" + mousePosition.X;
            this.initialMousePosition = mousePosition;
            previousMousePosition = initialMousePosition;
            //initialLineEndPosition = new Point(currentApproachLine.Line.X2, currentApproachLine.Line.Y2);
            this.isMouseDown = true;
            this.toMove = (UIElement)o;
            
            //GetDirectionFromLine((Line)o).LineWithText.EndSnapped = false;
            DeSnapLine((Line)o);

        }

        public bool DeSnapLine(Line line)
        {
            foreach (ApproachWithFan approachWithFan in Approaches)
            {
                if (approachWithFan.LineWithText.Line == line)
                {
                    approachWithFan.LineWithText.EndSnapped = false;
                    return true;
                }
                foreach (Direction direction in approachWithFan.Fan)
                {
                    if (direction.LineWithText.Line == line)
                    {
                        direction.LineWithText.EndSnapped = false;
                        return true;
                    }
                }
            }
            return false;
        }

       /* public void SnapFans()
        {
            foreach (ApproachWithFan approach in Approaches)
            {
                foreach (LineWithText feather in approach.Fan)
                {
                    foreach (ApproachWithFan otherApproach in Approaches)
                    {
                        foreach (LineWithText otherFeather in otherApproach.Fan)
                        {
                            if (feather.Line.X2 - otherFeather.Line.X1 < 1 && (feather.Line.Y2 - otherFeather.Line.Y1 < 1) && feather != otherFeather)
                            {
                                feather.Line.X2 = otherFeather.Line.X1;
                                feather.Line.Y2 = otherFeather.Line.Y1;
                                feather.EndSnapped = true;
                            }
                        }
                    }
                }
            }
        }*/

        public Direction GetDirectionFromLine(Line line)
        {
            foreach (ApproachWithFan approachWithFan in Approaches)
            {
                foreach (Direction direction in approachWithFan.Fan)
                {
                    if (direction.LineWithText.Line == line) return direction;
                }
            }
            return null;
        }
        public void SnapFeather(Direction feather)
        {
            foreach (ApproachWithFan otherApproach in Approaches)
            {
                foreach (Direction otherFeather in otherApproach.Fan)
                {
                    if (Math.Abs(feather.LineWithText.Line.X2 - otherFeather.LineWithText.Line.X1) < 5 && (Math.Abs(feather.LineWithText.Line.Y2 - otherFeather.LineWithText.Line.Y1) < 5) && feather != otherFeather && !(feather.LineWithText.Line.X1 == otherFeather.LineWithText.Line.X1 && feather.LineWithText.Line.Y1 == otherFeather.LineWithText.Line.Y1))
                    {
                        feather.LineWithText.Line.X2 = otherFeather.LineWithText.Line.X1;
                        feather.LineWithText.Line.Y2 = otherFeather.LineWithText.Line.Y1;
                        feather.LineWithText.EndSnapped = true;
                        otherApproach.SnappedFeathers.Add(feather);
                        otherApproach.LineWithText.Text.Text = "" + otherApproach.Outflow();
                        toMove = null;
                        isMouseDown = false;
                        return;
                    }
                }
                otherApproach.SnappedFeathers.RemoveAll(x => x == feather);
                feather.LineWithText.EndSnapped = false;
            }
            feather.LineWithText.EndSnapped = false;
            
        }
    }
}
