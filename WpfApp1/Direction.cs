using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace WpfApp1
{

   public class Direction
    {
        public string ID { get; set; }
        public string Name { get; set; }

        public string DispName
        {
            get
            {
                return Name;
            }

            set
            {

            }
        }
        public LineWithText LineWithText;
        private FlatFileRecord _refRecord;
       // private int _flow;

        
        public FlatFileRecord ReferenceRecord
        {
            get
            {
                return _refRecord;
            }
            set
            {
                _refRecord = value;
            }
        }

        public void SetLineWithText(LineWithText input)
        {
            LineWithText = input;
        }
        public LineWithText GetLineWithText()
        {
            return LineWithText;
        }
        public int Flow
        {
            get
            {
                return _refRecord.Count ;
            }
            set
            {
                _refRecord.Count = value;
            }
        }

        public DateTime DateTime
        {
            get
            {
                return _refRecord.DateTime;
            }

            set
            {

            }
        }

        public Direction(LineWithText inputLine, int flow, FlatFileRecord record)
        {
            Name = inputLine.Text.Text;
            LineWithText = inputLine;
            ReferenceRecord = record;
            Flow = flow;
            LineWithText.Text.Text = Name + ": " + Flow;

            
            var frmMain = ((MainWindow)Application.Current.MainWindow);
            LineWithText.Line.MouseDown += frmMain.mouseDownToMoveLine;

            frmMain.MouseMove += (o, s) =>
            {
                if (!frmMain.imageCanvas.Children.Contains(this.LineWithText.Line))
                {
                    return;
                }
                if (s.LeftButton == System.Windows.Input.MouseButtonState.Pressed && frmMain.toMove != null && !this.LineWithText.EndSnapped)
                {
                    var snapLine = frmMain.GetDirectionFromLine(frmMain.network.Intersections[_refRecord.IntersectionName], (Line)frmMain.toMove);
                    if (snapLine != null) frmMain.SnapFeather(frmMain.network.Intersections[_refRecord.IntersectionName], snapLine);
                }
            };

        }

        public Direction(LineWithText inputLine)
        {
            Name = inputLine.Text.Text;
            LineWithText = inputLine;
            Flow = 0;
            LineWithText.Text.Text = Name + ": " + Flow;
        }

        public void UpdateText()
        {
            LineWithText.Text.Text = Name + ": " + Flow;
        }
    }
}
