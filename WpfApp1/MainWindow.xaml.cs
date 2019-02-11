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
        private bool isMouseDown = false;
        private int clickNum = 0;
        private Point lineStart;
        private Intersection startIntersection;
        private LinkedList<FlatFileRecord> records = new LinkedList<FlatFileRecord>();
        private RoadNetwork network = new RoadNetwork();
        private List<UIElement> renderList = new List<UIElement>();


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
            if (isMouseDown)
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
            var zoomFactor = Math.Sign(e.Delta) * .05 + 1;

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

            network.intersections.Add(new Intersection((string)lstOut.SelectedValue, myCircle));

            TextBlock myCircleText = new TextBlock
            {
                Text = network.intersections.Last().Name,
                Foreground = Brushes.White,
                FontSize = 20
      
            };

            Canvas.SetLeft(myCircleText, e.GetPosition(imageCanvas).X + myCircle.Width / 2);
            Canvas.SetTop(myCircleText, e.GetPosition(imageCanvas).Y - myCircle.Height / 2);
            Panel.SetZIndex(myCircleText, 2);
            imageCanvas.Children.Add(myCircleText);

            lstOut.SelectedIndex++;
            lstOut.Focus();
            if (lstOut.Items.Count > 0) frmMain.Title = network.intersections.Last().Name;

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
                           startIntersection = network.intersections.Single(x => x.ellipse == myCircle);
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

                            double rotationAngle = (180 / Math.PI) * Math.Atan((line.Y2 - line.Y1)/ (line.X2 - line.X1));
                            if (line.X2 - line.X1 == 0) rotationAngle = 0;

                            Panel.SetZIndex(line, 0);
                            imageCanvas.Children.Add(line);

                            Intersection end = network.intersections.Single(x => x.ellipse == myCircle);
                            network.links.Add(new Link(startIntersection, end, line));
                            startIntersection.links.Add(network.links.Last());
                            end.links.Add(network.links.Last());

                            string roadName = "No common road name";
                            string[] startNames = startIntersection.Name.Replace(" ", string.Empty).Split('-');
                            string[] endNames = end.Name.Split('-');

                           
                            foreach (var startName in startNames)
                            {
                                if (end.Name.Replace(" ", string.Empty).Contains(startName))
                                {
                                    roadName = startName;
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
                       } break;
                   default:
                       {

                       } break;
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

            foreach (Intersection intersection in network.intersections)
            {
                intersection.SL = records.Single(x => x.IntersectionName == intersection.Name && x.Approach == "South" && x.Direction == "Left" && x.CommuterClass == (string)cbxClasses.SelectedValue && x.DateTime.TimeOfDay == (TimeSpan)cbxTimes.SelectedValue).Count;
                intersection.SS = records.Single(x => x.IntersectionName == intersection.Name && x.Approach == "South" && x.Direction == "Straight" && x.CommuterClass == (string)cbxClasses.SelectedValue && x.DateTime.TimeOfDay == (TimeSpan)cbxTimes.SelectedValue).Count;
                intersection.SR = records.Single(x => x.IntersectionName == intersection.Name && x.Approach == "South" && x.Direction == "Right" && x.CommuterClass == (string)cbxClasses.SelectedValue && x.DateTime.TimeOfDay == (TimeSpan)cbxTimes.SelectedValue).Count;
                intersection.EL = records.Single(x => x.IntersectionName == intersection.Name && x.Approach == "East" && x.Direction == "Left" && x.CommuterClass == (string)cbxClasses.SelectedValue && x.DateTime.TimeOfDay == (TimeSpan)cbxTimes.SelectedValue).Count;
                intersection.ES = records.Single(x => x.IntersectionName == intersection.Name && x.Approach == "East" && x.Direction == "Straight" && x.CommuterClass == (string)cbxClasses.SelectedValue && x.DateTime.TimeOfDay == (TimeSpan)cbxTimes.SelectedValue).Count;
                intersection.ER = records.Single(x => x.IntersectionName == intersection.Name && x.Approach == "East" && x.Direction == "Right" && x.CommuterClass == (string)cbxClasses.SelectedValue && x.DateTime.TimeOfDay == (TimeSpan)cbxTimes.SelectedValue).Count;
                intersection.WL = records.Single(x => x.IntersectionName == intersection.Name && x.Approach == "West" && x.Direction == "Left" && x.CommuterClass == (string)cbxClasses.SelectedValue && x.DateTime.TimeOfDay == (TimeSpan)cbxTimes.SelectedValue).Count;
                intersection.WS = records.Single(x => x.IntersectionName == intersection.Name && x.Approach == "West" && x.Direction == "Straight" && x.CommuterClass == (string)cbxClasses.SelectedValue && x.DateTime.TimeOfDay == (TimeSpan)cbxTimes.SelectedValue).Count;
                intersection.WR = records.Single(x => x.IntersectionName == intersection.Name && x.Approach == "West" && x.Direction == "Right" && x.CommuterClass == (string)cbxClasses.SelectedValue && x.DateTime.TimeOfDay == (TimeSpan)cbxTimes.SelectedValue).Count;
                intersection.NL = records.Single(x => x.IntersectionName == intersection.Name && x.Approach == "North" && x.Direction == "Left" && x.CommuterClass == (string)cbxClasses.SelectedValue && x.DateTime.TimeOfDay == (TimeSpan)cbxTimes.SelectedValue).Count;
                intersection.NS = records.Single(x => x.IntersectionName == intersection.Name && x.Approach == "North" && x.Direction == "Straight" && x.CommuterClass == (string)cbxClasses.SelectedValue && x.DateTime.TimeOfDay == (TimeSpan)cbxTimes.SelectedValue).Count;
                intersection.NR = records.Single(x => x.IntersectionName == intersection.Name && x.Approach == "North" && x.Direction == "Right" && x.CommuterClass == (string)cbxClasses.SelectedValue && x.DateTime.TimeOfDay == (TimeSpan)cbxTimes.SelectedValue).Count;

                TextBlock westFlow = new TextBlock
                {
                    Text = intersection.WestInFlow() + " -->>" + intersection.WestOutFlow(),
                    Foreground = Brushes.Black,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    FontSize = 12

                };

                //imageCanvas.Children.Add(westFlow);

                Point translated =  intersection.ellipse.TranslatePoint(new Point(0, 0), imageCanvas);

                Canvas.SetLeft(westFlow, translated.X);
                Canvas.SetTop(westFlow, translated.Y);


                UpdateLayout();
                Panel.SetZIndex(westFlow, 4);
            }

            foreach (Link link in network.links)
            {
                TextBlock startInlinkFlow = new TextBlock
                {
                    Text = link.StartInflow() + "",// + " -->>" + link.EndOutflow() + "\r\n" + link.StartOutflow() + "-->>" + link.EndInflow(),
                    Foreground = Brushes.Black,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    FontSize = 12
                    //A Nice comment
                };

                TextBlock startOutlinkFlow = new TextBlock
                {
                    Text = link.StartOutflow() + "",// + " -->>" + link.EndOutflow() + "\r\n" + link.StartOutflow() + "-->>" + link.EndInflow(),
                    Foreground = Brushes.Black,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    FontSize = 12

                }; //Extra Test Comment

                TextBlock EndInlinkFlow = new TextBlock
                {
                    Text = link.EndInflow() + "",// + " -->>" + link.EndOutflow() + "\r\n" + link.StartOutflow() + "-->>" + link.EndInflow(),
                    Foreground = Brushes.Black,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    FontSize = 12

                };

                TextBlock EndOutlinkFlow = new TextBlock
                {
                    Text = link.EndOutflow() + "",// + " -->>" + link.EndOutflow() + "\r\n" + link.StartOutflow() + "-->>" + link.EndInflow(),
                    Foreground = Brushes.Black,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    FontSize = 12

                };

                {
                    Line startOutFlow = new Line();
                    startOutFlow.Visibility = Visibility.Visible;
                    startOutFlow.Stroke = new SolidColorBrush(Color.FromArgb(150, 0, 254, 0));
                    var thickness = 10 * Math.Log(link.StartOutflow() / 2);
                    if (double.IsNaN(thickness) || double.IsInfinity(thickness) || thickness < 0) thickness = 0;
                    startOutFlow.StrokeThickness = thickness;
                    if (double.IsNaN(startOutFlow.StrokeThickness)) startOutFlow.StrokeThickness = 0;
                    var dx = link.Line.X2 - link.Line.X1;
                    var dy = link.Line.Y2 - link.Line.Y1;

                    startOutFlow.X1 = link.Line.X1 - thickness * Math.Sin(link.orientation()) / 2;
                    startOutFlow.Y1 = link.Line.Y1 + thickness * Math.Cos(link.orientation()) / 2;
                    startOutFlow.X2 = link.Line.X2 - thickness * Math.Sin(link.orientation()) / 2 - dx / 2;
                    startOutFlow.Y2 = link.Line.Y2 + thickness * Math.Cos(link.orientation()) / 2 - dy / 2;

                    imageCanvas.Children.Add(startOutFlow);
                    renderList.Add(startOutFlow);
                    Panel.SetZIndex(startOutFlow, 0);

                    imageCanvas.Children.Add(startOutlinkFlow);
                    renderList.Add(startOutlinkFlow);
                    Point translated = new Point((startOutFlow.X1 + startOutFlow.X2) / 2, (startOutFlow.Y1 + startOutFlow.Y2) / 2); //intersection.ellipse.TranslatePoint(new Point(0, 0), imageCanvas);
                    Canvas.SetLeft(startOutlinkFlow, translated.X);
                    Canvas.SetTop(startOutlinkFlow, translated.Y);
                    Panel.SetZIndex(startOutlinkFlow, 4);
                }

                {
                    Line endInFlow = new Line();
                    endInFlow.Visibility = Visibility.Visible;
                    endInFlow.Stroke = new SolidColorBrush(Color.FromArgb(150, 254, 254, 0));
                    var thickness = 10 * Math.Log(link.EndInflow() / 2);
                    if (double.IsNaN(thickness) || double.IsInfinity(thickness) || thickness < 0) thickness = 0;
                    endInFlow.StrokeThickness = thickness;
                    if (double.IsNaN(endInFlow.StrokeThickness)) endInFlow.StrokeThickness = 0;
                    var dx = link.Line.X2 - link.Line.X1;
                    var dy = link.Line.Y2 - link.Line.Y1;

                    endInFlow.X1 = link.Line.X1 - thickness * Math.Sin(link.orientation()) / 2 + dx / 2;
                    endInFlow.Y1 = link.Line.Y1 + thickness * Math.Cos(link.orientation()) / 2 + dy / 2;
                    endInFlow.X2 = link.Line.X2 - thickness * Math.Sin(link.orientation()) / 2;
                    endInFlow.Y2 = link.Line.Y2 + thickness * Math.Cos(link.orientation()) / 2;

                    imageCanvas.Children.Add(endInFlow);
                    renderList.Add(endInFlow);
                    Panel.SetZIndex(endInFlow, 0);

                    imageCanvas.Children.Add(EndInlinkFlow);
                    renderList.Add(EndInlinkFlow);
                    Point translated = new Point((endInFlow.X1 + endInFlow.X2) / 2, (endInFlow.Y1 + endInFlow.Y2) / 2); //intersection.ellipse.TranslatePoint(new Point(0, 0), imageCanvas);
                    Canvas.SetLeft(EndInlinkFlow, translated.X);
                    Canvas.SetTop(EndInlinkFlow, translated.Y);
                    Panel.SetZIndex(EndInlinkFlow, 4);
                }

                {
                    Line endOutFlow = new Line();
                    endOutFlow.Visibility = Visibility.Visible;
                    endOutFlow.Stroke = new SolidColorBrush(Color.FromArgb(150, 0, 254, 254));
                    var thickness = 10 * Math.Log(link.EndOutflow() / 2);
                    if (double.IsNaN(thickness) || double.IsInfinity(thickness) || thickness < 0) thickness = 0;
                    endOutFlow.StrokeThickness = thickness;
                    if (double.IsNaN(endOutFlow.StrokeThickness)) endOutFlow.StrokeThickness = 0;
                    var dx = link.Line.X2 - link.Line.X1;
                    var dy = link.Line.Y2 - link.Line.Y1;

                    endOutFlow.X1 = link.Line.X1 + thickness * Math.Sin(link.orientation()) / 2 + dx / 2;
                    endOutFlow.Y1 = link.Line.Y1 - thickness * Math.Cos(link.orientation()) / 2 + dy / 2;
                    endOutFlow.X2 = link.Line.X2 + thickness * Math.Sin(link.orientation()) / 2;
                    endOutFlow.Y2 = link.Line.Y2 - thickness * Math.Cos(link.orientation()) / 2;

                    imageCanvas.Children.Add(endOutFlow);
                    renderList.Add(endOutFlow);
                    Panel.SetZIndex(endOutFlow, 0);

                    imageCanvas.Children.Add(EndOutlinkFlow);
                    renderList.Add(EndOutlinkFlow);
                    Point translated = new Point((endOutFlow.X1 + endOutFlow.X2) / 2, (endOutFlow.Y1 + endOutFlow.Y2) / 2); //intersection.ellipse.TranslatePoint(new Point(0, 0), imageCanvas);
                    Canvas.SetLeft(EndOutlinkFlow, translated.X);
                    Canvas.SetTop(EndOutlinkFlow, translated.Y);
                    Panel.SetZIndex(EndOutlinkFlow, 4);
                }

                {
                    Line startInflow = new Line();
                    startInflow.Visibility = Visibility.Visible;
                    startInflow.Stroke = new SolidColorBrush(Color.FromArgb(150, 254, 0, 125));
                    var thickness = 10 * Math.Log(link.StartInflow() / 2);
                    if (double.IsNaN(thickness) || double.IsInfinity(thickness) || thickness < 0) thickness = 0;
                    startInflow.StrokeThickness = thickness;
                    if (double.IsNaN(startInflow.StrokeThickness)) startInflow.StrokeThickness = 0;
                    var dx = link.Line.X2 - link.Line.X1;
                    var dy = link.Line.Y2 - link.Line.Y1;

                    startInflow.X1 = link.Line.X1 + thickness * Math.Sin(link.orientation()) / 2;
                    startInflow.Y1 = link.Line.Y1 - thickness * Math.Cos(link.orientation()) / 2;
                    startInflow.X2 = link.Line.X2 + thickness * Math.Sin(link.orientation()) / 2 - dx / 2;
                    startInflow.Y2 = link.Line.Y2 - thickness * Math.Cos(link.orientation()) / 2 - dy / 2;

                    imageCanvas.Children.Add(startInflow);
                    renderList.Add(startInflow);
                    Panel.SetZIndex(startInflow, 0);

                    imageCanvas.Children.Add(startInlinkFlow);
                    renderList.Add(startInlinkFlow);
                    Point translated = new Point((startInflow.X1 + startInflow.X2) / 2, (startInflow.Y1 + startInflow.Y2) / 2); //intersection.ellipse.TranslatePoint(new Point(0, 0), imageCanvas);
                    Canvas.SetLeft(startInlinkFlow, translated.X);
                    Canvas.SetTop(startInlinkFlow, translated.Y);
                    Panel.SetZIndex(startInlinkFlow, 4);
                }

                


                UpdateLayout();
                
            }
        }


        private Intersection GetIntersectionFromCircle(UIElement circle)
        {
            return network.intersections.Single(x => x.ellipse == circle);
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
    }
}
