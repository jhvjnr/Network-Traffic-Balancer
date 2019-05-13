using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Xml.Serialization;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;


namespace WpfApp1
{

    public class Intersection
    {
        public Ellipse Ellipse { get; set; }
        public string ID {
            get
            {
                return Name;
            }
            set
            {
               // ID = value;
            }
        }
        public string Name { get; set; }
        public List<ApproachWithFan> Approaches { get; set; }
        

        public Intersection()
        {

        }

        public Intersection(string name, Ellipse inputEllipse)
        {
            Name = name;
            Ellipse = inputEllipse;
            Approaches = new List<ApproachWithFan>();

            var frmMain = ((MainWindow)Application.Current.MainWindow);

            Ellipse.MouseRightButtonDown += (o, s) =>
            {
                frmMain.Title = "add my info :)";
                var intEditor = new IntersectionEditor(frmMain.GetIntersectionFromCircle(Ellipse).Approaches);
                var relevantIntersection = frmMain.GetIntersectionFromCircle(Ellipse);
                intEditor.IntersectionName = relevantIntersection.Name;
                /* foreach (ApproachWithFan approach in relevantIntersection.Approaches)
                 {
                     intEditor.Approaches.Add(approach);
                 }*/
                intEditor.Intersection = relevantIntersection;
                //intEditor.Approaches = relevantIntersection.Approaches;
                frmMain.imageCanvas.Children.Add(intEditor);
                Canvas.SetLeft(intEditor, Ellipse.TranslatePoint(new Point(0, 0), frmMain.imageCanvas).X - Ellipse.Width / 2);
                Canvas.SetTop(intEditor, Ellipse.TranslatePoint(new Point(0, 0), frmMain.imageCanvas).Y - Ellipse.Height / 2);
                Panel.SetZIndex(intEditor, 30);
            };
        }

    }
}