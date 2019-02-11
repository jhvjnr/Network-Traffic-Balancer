using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace WpfApp1
{
    [Serializable]
    public class Intersection
    {
        public Ellipse ellipse { get; set; }
        public string Name { get; set; }
        public List<Link> links { get; set; }


        public double SR { get => sR; set => sR = value; }
        public double SS { get => sS; set => sS = value; }
        public double SL { get => sL; set => sL = value; }
        public double NR { get => nR; set => nR = value; }
        public double NS { get => nS; set => nS = value; }
        public double NL { get => nL; set => nL = value; }
        public double ER { get => eR; set => eR = value; }
        public double ES { get => eS; set => eS = value; }
        public double EL { get => eL; set => eL = value; }
        public double WR { get => wR; set => wR = value; }
        public double WS { get => wS; set => wS = value; }
        public double WL { get => wL; set => wL = value; }

        private double wL;
        private double sR;
        private double sS;
        private double sL;
        private double nR;
        private double nS;
        private double nL;
        private double eR;
        private double eS;
        private double eL;
        private double wR;
        private double wS;

        public Intersection()
        {

        }

        public Intersection(string name, Ellipse inputEllipse)
        {
            Name = name;
            ellipse = inputEllipse;
            links = new List<Link>();
        }

        public Link GetEastLink()
        {
            Link output = null;
            double maxEast = double.NegativeInfinity;

            foreach (Link link in links)
            {
                double otherIntersectionEast = double.NegativeInfinity;

                if (link.StartIntersection == this)
                {
                    otherIntersectionEast = link.Line.X2;
                }
                else
                {
                    otherIntersectionEast = link.Line.X1;
                }

                if (otherIntersectionEast > maxEast)
                {
                    maxEast = otherIntersectionEast;
                    output = link;
                }
            }
            return output;
        }

        public Link GetSouthLink()
        {
            Link output = null;
            double maxSouth = double.NegativeInfinity;

            foreach (Link link in links)
            {
                double otherIntersectionSouth = double.NegativeInfinity;

                if (link.StartIntersection == this)
                {
                    otherIntersectionSouth = link.Line.Y2;
                }
                else
                {
                    otherIntersectionSouth = link.Line.Y1;
                }

                if (otherIntersectionSouth < maxSouth)
                {
                    maxSouth = otherIntersectionSouth;
                    output = link;
                }
            }
            return output;
        }

        public Link GetNorthLink()
        {
            Link output = null;
            double maxNorth = double.PositiveInfinity;

            foreach (Link link in links)
            {
                double otherIntersectionNorth = double.PositiveInfinity;

                if (link.StartIntersection == this)
                {
                    otherIntersectionNorth = link.Line.Y2;
                }
                else
                {
                    otherIntersectionNorth = link.Line.Y1;
                }

                if (otherIntersectionNorth < maxNorth)
                {
                    maxNorth = otherIntersectionNorth;
                    output = link;
                }
            }
            return output;
        }

        public Link GetWestLink()
        {
            Link output = null;
            double maxWest = double.PositiveInfinity;

            foreach (Link link in links)
            {
                double otherIntersectionWest = double.PositiveInfinity;

                if (link.StartIntersection == this)
                {
                    otherIntersectionWest = link.Line.X2;
                }
                else
                {
                    otherIntersectionWest = link.Line.X1;
                }

                if (otherIntersectionWest < maxWest)
                {
                    maxWest = otherIntersectionWest;
                    output = link;
                }
            }
            return output;
        }

        public double SouthOutFlow()
        {
            return WR + NS + EL;
        }

        public double EastOutFlow()
        {
            return WS + NL + SR;
        }

        public double WestOutFlow()
        {
            return ES + NR + SL;
        }

        public double NorthOutFlow()
        {
            return WL + SS + ER;
        }

        public double SouthInFlow()
        {
            return SR + SS + SL;
        }

        public double EastInFlow()
        {
            return ES + EL + ER;
        }

        public double WestInFlow()
        {
            return WS + WR + WL;
        }

        public double NorthInFlow()
        {
            return NL + NS + NR;
        }

        public void imageDataToLinks()
        {
          /*  if (GetWestLink().isInward(this))
            {
                
            }*/
        }
    }
}
