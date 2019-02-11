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
        public Ellipse ellipse { get; set; }
        public string Name { get; set; }
        public List<Link> links { get; set; }

        public Matrix<double> IOMatrix { get; set; }
            /*= DenseMatrix.OfArray(new double[,]
        {
            {1,2 },
            {2,1 }
        });*/

        

        public Intersection()
        {

        }

        public Intersection(string name, Ellipse inputEllipse)
        {
            Name = name;
            ellipse = inputEllipse;
            links = new List<Link>();
        }
    }
}
