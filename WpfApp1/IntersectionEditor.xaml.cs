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

        public string IntersectionName
        {
            get
            {
                return intersection.Name;
            }
            set
            {
                
            }
        }

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
                //approaches = value;
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
           // ((Canvas)Parent).Children.Remove(this);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            //grdData.rowv
            var window = ((MainWindow)Application.Current.MainWindow);
            foreach (DataGrid grid in FindVisualChildren<DataGrid>(grdData))
            {
                if (HasError(grid))
                {
                    MessageBox.Show("Please fix all invalid cells :)");
                    return;
                }
            }

            if (HasError(grdData) )
            {
                MessageBox.Show("Please fix all invalid cells :)");
                return;
            }
            ((Canvas)Parent).Children.Remove(this);
            foreach (ApproachWithFan approach in approaches)
            {
                approach.UpdateVisuals();
            }

            window.RedrawInterDatas();
        }

        private bool HasError(DataGrid dg)
        {
            bool errors = (from c in
                             (from object i in dg.ItemsSource
                              select dg.ItemContainerGenerator.ContainerFromItem(i))
                           where c != null
                           select Validation.GetHasError(c)
                          ).FirstOrDefault(x => x);
            return errors;
        }


        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
