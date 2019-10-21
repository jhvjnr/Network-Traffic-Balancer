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
using System.Windows.Markup;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Point initialMousePosition;
        public Point previousMousePosition;
        private Point initialImagePosition;
        public UIElement toMove;
       // public bool isMouseDown = false;
        private int clickNum = 0;
        private Point lineStart;
        private Intersection startIntersection;
        private LinkedList<FlatFileRecord> records = new LinkedList<FlatFileRecord>();
        //private RoadNetwork network = new RoadNetwork();
        private List<UIElement> renderList = new List<UIElement>();
        //private List<ApproachWithFan> Approaches = new List<ApproachWithFan>();
        public List<InterApproachData> InterApproachDatas = new List<InterApproachData>();
        //private Dictionary<string, Intersection> Intersections = new Dictionary<string, Intersection>();
        public IntersectionNetwork network = new IntersectionNetwork();
        private double zoomFactor = 1;

        public MainWindow()
        {
            TileGenerator.CacheFolder = @"TempCache";
            InitializeComponent();
        }

        private void imgMap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }


        private void imgMap_MouseMove(object sender, MouseEventArgs e) //called mouse moves over image
        {                                       
            frmMain.Title = "mouse is moving over image";
            if (e.MiddleButton == MouseButtonState.Pressed && toMove == null) //Pans map if middle mouse button pressed and not trying to move anything else
            {
                Point deltaPos = new Point(e.GetPosition(this).X - previousMousePosition.X, e.GetPosition(this).Y - previousMousePosition.Y);
                var layoutMatrix = imageCanvas.RenderTransform as MatrixTransform;
                var matrix = layoutMatrix.Matrix;
                matrix.Translate(deltaPos.X, deltaPos.Y);
                layoutMatrix.Matrix = matrix;
                previousMousePosition = e.GetPosition(this);
                
            }
        }

        private void imgMap_MouseUp(object sender, MouseButtonEventArgs e)
        {
           // isMouseDown = false;
        }

        private void imgMap_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            
        }

        private void frmMain_MouseUp(object sender, MouseButtonEventArgs e) //We move nothing if mouse is not down
        {
            toMove = null;
        }

        private void imgMap_MouseRightButtonUp(object sender, MouseButtonEventArgs e) //Intersection placed on mouse right down
        {
            frmMain.Title = "Trying to place Circle";

            //only possible if intersections have been loaded
            if (network == null || lstOut.SelectedValue == null || network.Intersections.ContainsKey((string)lstOut.SelectedValue)) return;





            var myCircle = new Ellipse // this will be the ellipse drawn to represent the intersection
            {
                Stroke = Brushes.Blue,
                Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255)),
                Width = 50,
                Height = 50
            };




            imageCanvas.Children.Add(myCircle); //Ellipse is drawn onto the canvas
            Canvas.SetLeft(myCircle, e.GetPosition(imageCanvas).X - myCircle.Width / 2);
            Canvas.SetTop(myCircle, e.GetPosition(imageCanvas).Y - myCircle.Height / 2);
            imageCanvas.UpdateLayout();
            Panel.SetZIndex(myCircle, 7);


            var toAdd = new Intersection((string)lstOut.SelectedValue, myCircle);
            //new intersection object
            myCircle.MouseDown += (o, args) => // The ellipse's mouse down event is given the following code:
            {
                if (Keyboard.IsKeyDown(Key.X) && args.LeftButton == MouseButtonState.Pressed) // if x is pressed and ellipse is clicked, the intersection is deleted
                {
                    this.network.Intersections.Remove(toAdd.Name);
                    imageCanvas.Children.Remove(myCircle);
                    imageCanvas.Children.Remove(toAdd.Label);
                    foreach (ApproachWithFan approach in toAdd.Approaches)
                    {
                        imageCanvas.Children.Remove(approach.LineWithText.Line);
                        imageCanvas.Children.Remove(approach.LineWithText.Text);
                        imageCanvas.Children.Remove(approach.OutFlowIndicator.Line);
                        imageCanvas.Children.Remove(approach.InFlowIndicator.Line);
                        imageCanvas.Children.Remove(approach.OutFlowIndicator.Text);
                        imageCanvas.Children.Remove(approach.InFlowIndicator.Text);
                        foreach (Direction direction in approach.Fan)
                        {
                            imageCanvas.Children.Remove(direction.LineWithText.Line);
                            imageCanvas.Children.Remove(direction.LineWithText.Text);
                        }
                    }
                    GC.Collect();
                }
                
            };

            if (toAdd != null && toAdd.Name != null && !network.Intersections.ContainsKey(toAdd.Name))
            {
                network.Intersections.Add(toAdd.Name, toAdd); // new intersection is placed in dictionary of intersections called network
            }
            else
            {
                return;
            }




            lstOut.SelectedIndex++;
            lstOut.Focus();
            if (lstOut.Items.Count > 0) frmMain.Title = network.Intersections.Values.Last().Name;



            // The approaches that link to the intersection are identified
            var links = records.Where(x => x.IntersectionName == toAdd.Name && x.CommuterClass == (string)cbxClasses.SelectedItem && x.DateTime.TimeOfDay == (TimeSpan)cbxTimes.SelectedValue);
            if (links.Count() == 0) return;
            var approaches = links.GroupBy(x => x.Approach).Select(g => g.First());
            double divAngle = 360 / approaches.Count();
            double currentAngle = 90;


            // The approaches identified are drawn as lines spread evenly around the intersection:
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

                
              
                // The turning / through movements associated with the approach are identified and called directions:
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

                    var record = records.Single(x => x.IntersectionName == toAdd.Name && x.CommuterClass == (string)cbxClasses.SelectedItem && x.DateTime.TimeOfDay == (TimeSpan)cbxTimes.SelectedValue && x.Direction == direction.Direction && x.Approach == direction.Approach);
                    brush = new SolidColorBrush(ColorFromHSL(-currentAngle + 180, .5, .5));
                    Direction currentDirectionLine = new Direction(new LineWithText((Line)DrawLineOnCanvas(imageCanvas, startCoords, endCoords, brush, 4, 8), direction.Direction), direction.Count, record);
                    currentDirectionLine.GetLineWithText().Text.Text = currentDirectionLine.Name + ": " + currentDirectionLine.Flow;
                    fanLines.Add(currentDirectionLine);
                    


                    frmMain.MouseMove += (senderOf, args) =>
                    {
                        if (args.LeftButton == MouseButtonState.Pressed && toMove != null)
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
            
                frmMain.MouseMove += (o, s) =>
                {
                     if (s.LeftButton == MouseButtonState.Pressed && toMove != null && !currentApproachLine.EndSnapped)
                    {
                    var toSnapApproach = GetApproachWithFanFromLine((Line)toMove);
                    if (toSnapApproach != null) SnapApproach(toSnapApproach);
                    }
                    
                };
                newApproachWithFan.LineWithText.Text.Text = newApproachWithFan.Name;


                toAdd.Approaches.Add(newApproachWithFan);
                
                currentAngle += divAngle;
            }
          
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

        private void btnLoadIntersectionClick(object sender, EventArgs e) //Clicking this button loads content of CSV flat file containing intersection count data
        {

            var opn = new Microsoft.Win32.OpenFileDialog();
            opn.Filter = "CSV Files(*.csv) | *.csv";

            var result = opn.ShowDialog();

            try
            {
                if (result == true)
                {
                    records.Clear();
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
            catch
            {
                MessageBox.Show("Error loading input file :(");
            }
        }

       private void loadCounts() // loads appropriate counts for selected vehicle class and period times
        {
            frmMain.Title = "Getting new data";
            foreach (Intersection intersection in network.Intersections.Values)
            {
                foreach (ApproachWithFan approach in intersection.Approaches)
                {
                    string test = "" + approach.Inflow;
                    approach.Inflow = records.Where(x => x.IntersectionName == intersection.Name && approach.Name == x.Approach && x.CommuterClass == (string) cbxClasses.SelectedValue && x.DateTime.TimeOfDay == (TimeSpan) cbxTimes.SelectedValue).Sum(g => g.Count);
                    test += " -> " + approach.Inflow;

                    foreach (Direction direction in approach.Fan)
                    {
                        var numMatching =  records.Count(x => x.IntersectionName == intersection.Name && approach.Name == x.Approach && x.CommuterClass == (string)cbxClasses.SelectedValue && x.DateTime.TimeOfDay == (TimeSpan)cbxTimes.SelectedValue && direction.Name == x.Direction);

                        if (numMatching != 1)
                        {
                            direction.ReferenceRecord = new FlatFileRecord(intersection.Name, records.First().DateTime.Date.Add((TimeSpan) cbxTimes.SelectedValue), approach.Name, direction.Name, (string) cbxClasses.SelectedValue, 0);
                            direction.UpdateText();
                            continue;
                        }
                        var flow = records.Single(x => x.IntersectionName == intersection.Name && approach.Name == x.Approach && x.CommuterClass == (string)cbxClasses.SelectedValue && x.DateTime.TimeOfDay == (TimeSpan)cbxTimes.SelectedValue && direction.Name == x.Direction);

    
                        direction.ReferenceRecord = flow;
                        direction.UpdateText();
                    }
                }
            }

            foreach (Intersection intersection in network.Intersections.Values)
            {
                foreach (ApproachWithFan approach in intersection.Approaches)
                {
                    approach.DrawIndicators();
                }
            }
        }


        public Intersection GetIntersectionFromCircle(UIElement circle) //Returns intersection that corresponds with given ellipse
        {
            return network.Intersections.Values.Single(x => x.Ellipse == circle);
        }

        private void btnAssignIntersections(object sender, EventArgs e)
        {

        }

        private void BtnAddNumbers(object sender, EventArgs e)
        {
            loadCounts();
        }

        private void cbxTimes_SelectionChanged(object sender, SelectionChangedEventArgs e) // called when selected time period changes
        {
            loadCounts();
        }

        private void cbxClasses_SelectionChanged(object sender, SelectionChangedEventArgs e) // called when selected vehicle class changes
        {
            loadCounts();
        }

        public static UIElement DrawLineOnCanvas(Canvas canvas, Point startCoords, Point endCoords, Brush brush, double thickness, int renderpriority)
        { // can be used to draw line on given canvas and returns the line drawn's reference
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

            canvas.Children.Add(line);

            double rotationAngle = (180 / Math.PI) * Math.Atan((line.Y2 - line.Y1) / (line.X2 - line.X1));
            if (line.X2 - line.X1 == 0) rotationAngle = 0;

            Panel.SetZIndex(line, renderpriority);
           

            return line;
        }

       public static Color ColorFromHSL(double H, double S, double L) // returns colour corresponding to hue angle given
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
        }

        public void mouseDownToMoveLine(object o, MouseEventArgs args) // Idenfies and saves what line is to be moved
        {
            Point mousePosition = args.GetPosition(this);
            frmMain.Title = "I am trying to move the line :)" + mousePosition.X;
            this.initialMousePosition = mousePosition;
            previousMousePosition = initialMousePosition;

            this.toMove = (UIElement)o;

            DeSnapLine((Line)o);

        }

        public bool DeSnapApproach(ApproachWithFan approach) // Desnaps approach from other
        {
            approach.EndSnapped = false;
            List<InterApproachData> toRemove = new List<InterApproachData>();
            if (approach.SnappedApproach != null) approach.SnappedApproach.EndSnapped = false;
            foreach (var interApproachData in InterApproachDatas)
            {
                if (interApproachData.Approach1.Equals(approach) || interApproachData.Approach2.Equals(approach))
                {
                    imageCanvas.Children.Remove(interApproachData);
                    toRemove.Add(interApproachData);
                }
            }
            InterApproachDatas.RemoveAll(x => toRemove.Contains(x));
            return true;
        }

        public bool DeSnapLine(Line line) // desnaps line from other
        {
            foreach (Intersection intersection in network.Intersections.Values)
            {
                foreach (ApproachWithFan approachWithFan in intersection.Approaches)
                {
                    if (approachWithFan.LineWithText.Line == line)
                    {
                        approachWithFan.LineWithText.EndSnapped = false;
                        return true;
                    }
                    foreach (Direction direction in approachWithFan.Fan)
                    {
                        if (direction.GetLineWithText().Line == line)
                        {
                            direction.GetLineWithText().EndSnapped = false;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

      

        public Direction GetDirectionFromLine(Intersection intersection, Line line) // returns Direction that corresponds to given intersection and UIElement line
        {
            foreach (ApproachWithFan approachWithFan in intersection.Approaches)
            {
                foreach (Direction direction in approachWithFan.Fan)
                {
                    if (direction.GetLineWithText().Line == line) return direction;
                }
            }
            return null;
        }

        public ApproachWithFan GetApproachWithFanFromLine(Line line) // returns approach that corresponds to given UIElement line
        {
            foreach (Intersection intersection in network.Intersections.Values)
            {
                foreach (ApproachWithFan approach in intersection.Approaches)
                {
                    if (approach.LineWithText.Line == line) return approach;
                }
            }
            return null;
        }


        public void SnapApproach(ApproachWithFan approach) // snaps approaches together if they are close
        {
            foreach (Intersection intersection in network.Intersections.Values)
            {
                foreach (ApproachWithFan otherApproach in intersection.Approaches)
                {
                    if (Math.Abs(approach.LineWithText.Line.X2 - otherApproach.LineWithText.Line.X2) < 5 && (Math.Abs(approach.LineWithText.Line.Y2 - otherApproach.LineWithText.Line.Y2) < 5) && approach != otherApproach && !(approach.LineWithText.Line.X1 == otherApproach.LineWithText.Line.X1 && approach.LineWithText.Line.Y1 == otherApproach.LineWithText.Line.Y1 && otherApproach.LineWithText.EndSnapped == false && approach.LineWithText.EndSnapped == false && otherApproach.EndSnapped == false && approach.EndSnapped == false))
                    {
                        approach.LineWithText.Line.X2 = otherApproach.LineWithText.Line.X2;
                        approach.LineWithText.Line.Y2 = otherApproach.LineWithText.Line.Y2;
                        approach.LineWithText.EndSnapped = true;
                        otherApproach.SnappedApproach = approach;
                        approach.SnappedApproach = otherApproach;

                        
                        toMove = null;
                       
                        var dx = approach.LineWithText.Line.X1 - otherApproach.LineWithText.Line.X1;
                        var dy = approach.LineWithText.Line.Y1 - otherApproach.LineWithText.Line.Y1;
                        var startCoords = new Point(approach.LineWithText.Line.X2 - (50), approach.LineWithText.Line.Y2);
                        var endCoords = new Point(approach.LineWithText.Line.X2 + (50), approach.LineWithText.Line.Y2);
                        var brush = Brushes.White;
                        
                        var simOutPercentage = 100 * Math.Abs(approach.Outflow - otherApproach.Inflow) / ((approach.Outflow + otherApproach.Inflow) / 2f);
                        var simInPercentage = 100 * Math.Abs(approach.Inflow - otherApproach.Outflow) / ((approach.Inflow + otherApproach.Outflow) / 2f);

                        var newInterApproachControl = new InterApproachData(approach, otherApproach);
                        imageCanvas.Children.Add(newInterApproachControl);
                       
                        Panel.SetZIndex(newInterApproachControl, 5);
                        Canvas.SetLeft(newInterApproachControl, approach.LineWithText.Line.X2 - newInterApproachControl.circleRadius);
                        Canvas.SetTop(newInterApproachControl, approach.LineWithText.Line.Y2 - newInterApproachControl.circleRadius);

                        return;
                    }
                   
                    otherApproach.SnappedApproach = null ;
                    approach.SnappedApproach = null;
                    otherApproach.LineWithText.EndSnapped = false;
                    approach.LineWithText.EndSnapped = false;
                    
                    
                }
                approach.LineWithText.EndSnapped = false;
            }
        }


        public void SnapFeather(Intersection intersection, Direction feather) // Snaps Directions (feathers) together if they are close)
        {
            foreach (ApproachWithFan otherApproach in intersection.Approaches)
            {
                var otherFeather = otherApproach.Fan.First();

                    if (Math.Abs(feather.GetLineWithText().Line.X2 - otherFeather.GetLineWithText().Line.X1) < 5 && (Math.Abs(feather.GetLineWithText().Line.Y2 - otherFeather.GetLineWithText().Line.Y1) < 5) && feather != otherFeather && !(feather.GetLineWithText().Line.X1 == otherFeather.GetLineWithText().Line.X1 && feather.GetLineWithText().Line.Y1 == otherFeather.GetLineWithText().Line.Y1))
                    {
                        feather.GetLineWithText().Line.X2 = otherFeather.GetLineWithText().Line.X1;
                        feather.GetLineWithText().Line.Y2 = otherFeather.GetLineWithText().Line.Y1;
                        feather.GetLineWithText().EndSnapped = true;
                        otherApproach.SnappedFeathers.Add(feather);
                       
                        toMove = null;
                       
                        otherApproach.DrawIndicators();                    
                    return;
                    }
                
                otherApproach.SnappedFeathers.Remove(feather);
                feather.GetLineWithText().EndSnapped = false;
                otherApproach.DrawIndicators();

            }
            feather.GetLineWithText().EndSnapped = false;
        }

        public void RedrawInterDatas() // When counts change, call this to redraw the inter approach indicators
        {
            List<Tuple<ApproachWithFan, ApproachWithFan>> newTups = new List<Tuple<ApproachWithFan, ApproachWithFan>>();

            foreach (InterApproachData comp in InterApproachDatas)
            {
                imageCanvas.Children.Remove(comp);
                var newTup = new Tuple<ApproachWithFan, ApproachWithFan>(comp.Approach1, comp.Approach2);
                
                newTups.Add(newTup);

            }

            InterApproachDatas.Clear();
            foreach (Tuple<ApproachWithFan, ApproachWithFan> tup in newTups)
            {
                var newComp = new InterApproachData(tup.Item1, tup.Item2);
                imageCanvas.Children.Add(newComp);
                Panel.SetZIndex(newComp, 5);
                Canvas.SetLeft(newComp, newComp.Approach1.LineWithText.Line.X2 - newComp.circleRadius);
                Canvas.SetTop(newComp, newComp.Approach1.LineWithText.Line.Y2 - newComp.circleRadius);
                InterApproachDatas.Add(newComp);
            }
        }

        private void frmMain_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        { // Saves edited data to file
          

            var sve = new Microsoft.Win32.SaveFileDialog();
            var result = sve.ShowDialog();

            if (result == true)
            {
                var writer = new StreamWriter(sve.FileName);
                writer.WriteLine(",Intersection,Date,Approach,Direction,Class,Count");
                foreach (var record in records)
                {
                    writer.Write(record + "\r\n");
                }
                writer.Close();
                writer.Dispose();
            }

        }

        private void SaveCanvas()
        {
            string saved = XamlWriter.Save(imageCanvas);

        }

        private void btnSaveNetwork_Click(object sender, RoutedEventArgs e) // Saves current layout of network to xml file
        {
            var sve = new Microsoft.Win32.SaveFileDialog();
            var result = sve.ShowDialog();

            if (result == true)
            {

                network.WriteToFile(imageCanvas, sve.FileName);
            }

        }

        private void btnOpenNetwork_Click(object sender, RoutedEventArgs e)
        { // loads intersection layout from saved xml file


            if (records.Count == 0)
            {
                MessageBox.Show("Please load counts first :)");
                return;
            }
            var opn = new Microsoft.Win32.OpenFileDialog();

            opn.Filter = "XML files (*.xml, *.XML) | *.xml; *.XML;";
            var result = opn.ShowDialog();

            IntersectionNetwork testNet = null;
            try
            {
                if (result == true)
                {
                    testNet = IntersectionNetwork.LoadFromFile(opn.FileName, imageCanvas, records, this);
                }
            }
            catch
            {
                MessageBox.Show("Invalid network file :(");
                return;
            }
            imageCanvas.Children.Clear();
            imageCanvas.Children.Add(ZoomPanCanvas);
            InterApproachDatas.Clear();
            RedrawInterDatas();
            network = IntersectionNetwork.LoadFromFile(opn.FileName, imageCanvas, records, this);

            foreach (Intersection intersection in network.Intersections.Values)
            {
                foreach (ApproachWithFan approach in intersection.Approaches)
                {
                    SnapApproach(approach);
                    foreach (Direction feather in approach.Fan)
                    {
                        SnapFeather(intersection, feather);
                    }
                }
            }

        }

        private void frmMain_Activated(object sender, EventArgs a) // called when form opens for first time
        {
            frmMain.MouseWheel += (o, e) => // logic for zooming the image
            {
                frmMain.Title = "" + e.Delta;

                var layoutMatrix = imageCanvas.RenderTransform as MatrixTransform;
                var matrix = layoutMatrix.Matrix;
                zoomFactor = Math.Sign(e.Delta) * .05 + 1;

                matrix.ScaleAtPrepend(zoomFactor, zoomFactor, e.GetPosition(imageCanvas).X, e.GetPosition(imageCanvas).Y);
                layoutMatrix.Matrix = matrix;
            };

            frmMain.MouseDown += (o, e) => // logic for starting pan action
            {
                if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    Point mousePosition = e.GetPosition(this);
                    toMove = null;
                    frmMain.Title = "" + mousePosition.X;
                    this.initialMousePosition = mousePosition;
                    previousMousePosition = initialMousePosition;
                    initialImagePosition = new Point(Canvas.GetLeft(imageCanvas), Canvas.GetTop(imageCanvas));
                    //this.isMouseDown = true;
                }
            };

            frmMain.MouseMove += (senderOf, e) => // logic for dragging line action
            {
                if (e.LeftButton == MouseButtonState.Pressed && toMove != null)
                {
                    var lineToMove = (Line)toMove;
                    //Point deltaPos = new Point(e.GetPosition(this).X - previousMousePosition.X, e.GetPosition(this).Y - previousMousePosition.Y);
                    double dx = e.GetPosition(imageCanvas).X - lineToMove.X1;
                    double dy = e.GetPosition(imageCanvas).Y - lineToMove.Y1;
                    double angle = Math.Atan2(dx, dy);
                    lineToMove.X2 = e.GetPosition(imageCanvas).X + Math.Sin(angle) * lineToMove.StrokeThickness / 2;//+= deltaPos.X / zoomFactor;
                    lineToMove.Y2 = e.GetPosition(imageCanvas).Y + Math.Cos(angle) * lineToMove.StrokeThickness / 2; //+= deltaPos.Y / zoomFactor;
                    previousMousePosition = e.GetPosition(this);
                    frmMain.Title = "I am trying to move the line :)";
                }
            };
        }

        private void btnChangeBackground_Click(object sender, RoutedEventArgs e)
        { //Logic for loading a background image
            var opn = new Microsoft.Win32.OpenFileDialog();
            
            opn.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.tiff) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.tiff; |All Files|*.*";
            var result = opn.ShowDialog();
            try
            {
                if (result == true)
                {
                    imgMap.Source = new BitmapImage(new Uri(opn.FileName));
                }
            }
            catch
            {
                MessageBox.Show("Please open valid image :)");
            }
        }

        private void uiScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //logic for scaling the UI according to slider
            var scale = new ScaleTransform(uiScaleSlider.Value, uiScaleSlider.Value, imgMap.RenderTransformOrigin.X + ZoomPanCanvas.ActualWidth / 2, imgMap.RenderTransformOrigin.Y  + ZoomPanCanvas.ActualHeight / 2);
            imgMap.RenderTransform = scale;
        }
    }
}
