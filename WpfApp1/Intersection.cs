using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
namespace WpfApp1
{
    [Serializable]
    public class Intersection
    {
        public Ellipse Ellipse { get; set; }
        public string Name { get; set; }
        public List<Link> Links { get; set; }
        

        public Intersection()
        {

        }

        public Intersection(string name, Ellipse inputEllipse)
        {
            Name = name;
            Ellipse = inputEllipse;
            Links = new List<Link>();
        }
    }
}
