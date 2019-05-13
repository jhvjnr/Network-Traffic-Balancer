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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for IntersectionEditor.xaml
    /// </summary>
    public partial class IntersectionEditor : UserControl
    {

        public string IntersectionName { get; set; }

        private Intersection intersection;

        public Intersection Intersection
        {
            get
            {
                return intersection;
            }
            set
            {
                intersection = value;
            }
        }
        private List<ApproachWithFan> approaches = new List<ApproachWithFan>();

        public List<ApproachWithFan> Approaches
        {
            get
            {
                return approaches;
            }
            set
            {
                approaches = value;
            }
        }

        public IntersectionEditor(List<ApproachWithFan> inputApproaches)
        {
            //IntersectionName = "the Name :)";//
            InitializeComponent();
            approaches = inputApproaches;
            grdData.ItemsSource = approaches;

            DataContext = this;
            grdData.Items.Refresh();
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ((Canvas)Parent).Children.Remove(this);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            ((Canvas)Parent).Children.Remove(this);
            foreach (ApproachWithFan approach in approaches)
            {
                approach.UpdateVisuals();
            }
        }
    }
}
