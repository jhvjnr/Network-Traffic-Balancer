using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for InterApproachData.xaml
    /// </summary>
    public partial class InterApproachData : UserControl
    {

       public ApproachWithFan Approach1 { get; set; }
       public ApproachWithFan Approach2 { get; set; }
       public double circleRadius
        {
            get
            {
               return BackCircle.Width / 2;
            }
        }

        public double Arrow1Left
        {
            get
            {

                return  circleRadius - circleRadius * Math.Sin(Approach1.LineWithText.OrientationRAD());
            }
        }

        public Brush BackColor
        {
            get
            {
                if (Appr2InflowDif < 0.05 && Appr1InflowDif < 0.05) return Brushes.Green;
                return Brushes.Red;
            }
        }

        public double Arrow1Top
        {
            get
            {
                return  circleRadius - circleRadius * Math.Cos(Approach1.LineWithText.OrientationRAD());
            }
        }

        public double Arrow2Left
        {
            get
            {
                return circleRadius - circleRadius * Math.Sin(Approach2.LineWithText.OrientationRAD());
            }
        }

        public double Text1Top
        {
            get
            {
                return Arrow1Top;// + 3 * Math.Cos(Approach1.LineWithText.OrientationRAD());
            }
        }

        public double Text1Left
        {
            get
            {
                return Arrow1Left;// + 3 * Math.Sin(Approach1.LineWithText.OrientationRAD()); ;
            }
        }

        public double Arrow2Top
        {
            get
            {
                return circleRadius - circleRadius * Math.Cos(Approach2.LineWithText.OrientationRAD());
            }
        }
        
        public double Text1Angle
        {
            get
            {
                return Approach1Angle + 90;
            }
        }

        public double Text2Angle
        {
            get
            {
                return Approach2Angle + 90;
            }
        }


        public double Approach1Angle
        {
            get
            {
                return -Approach1.LineWithText.OrientationDEG();
            }
        }

        public double Approach2Angle
        {
            get
            {
                return -Approach2.LineWithText.OrientationDEG();
            }
        }

        public double Appr1Inflow
        {
            get
            {
                return Approach1.Inflow;
            }
            set
            {
               
            }
        }
        public double Appr2Inflow
        {
            get
            {
                return Approach2.Inflow;
            }
            set
            {

            }
        }
        public double Appr1Outflow
        {
            get
            {
                return Approach1.Outflow;
            }
            set
            {

            }
        }
        public double Appr2Outflow
        {
            get
            {
                return Approach2.Outflow;
            }
            set
            {

            }
        }

        public double Appr1InflowDif
        {
            get
            {
                var dv = Math.Abs(Appr1Inflow - Appr2Outflow);
                var ave = (Appr1Inflow + Appr2Outflow) / 2;
                return dv / ave;
            }
        }

        public double Appr2InflowDif
        {
            get
            {
                var dv = Math.Abs(Appr2Inflow - Appr1Outflow);
                var ave = (Appr2Inflow + Appr1Outflow) / 2;
                return dv / ave;
            }
        }

        public string Arrow2Text
        {
            get
            {
                string output = string.Format("{0:0.0}%", Appr2InflowDif * 100);
                return "" + output;
            }
        }

        public string Arrow1Text
        {
            get
            {
                string output = string.Format("{0:0.0}%", Appr1InflowDif * 100);
                return "" + output;
            }
        }


        public InterApproachData(ApproachWithFan approach1, ApproachWithFan approach2)//double appr1Inflow, double appr1Outflow, double appr2Inflow, double appr2Outflow)
        {
            Approach1 = approach1;
            Approach2 = approach2;
            DataContext = this;
            InitializeComponent();
            var frmMain = ((MainWindow)Application.Current.MainWindow);

            frmMain.InterApproachDatas.Add(this);
            /*Appr1Inflow = appr1Inflow;
            Appr2Inflow = appr2Inflow;
            Appr1Outflow = appr1Outflow;
            Appr2Outflow = appr2Outflow;*/


        }

        private void base_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
