using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace LKDS_Logger_NVRAM
{
    public partial class WindowFullLog : Window
    {
        LBAddConnect lBAddConnect = new LBAddConnect();
        LB CurrentLB;
        List<Dump> Dumps = new List<Dump>();
        public WindowFullLog()
        {
            InitializeComponent();
        }

        public WindowFullLog(LB lb)
        {
            InitializeComponent();
            Console.WriteLine("айди блока, у которого открыт полный лог:" + lb.LBId);
            CurrentLB = lb;
            TextBlockId.Text = CurrentLB.LBId.ToString();
            Dumps = lBAddConnect.GetAllDumps(CurrentLB.LBId);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
