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
using System.Windows.Shapes;

namespace LKDS_Logger_NVRAM
{
    /// <summary>
    /// Логика взаимодействия для WindowFullLog.xaml
    /// </summary>
    public partial class WindowFullLog : Window
    {
        public WindowFullLog()
        {
            InitializeComponent();
        }

        public WindowFullLog(LB lb)
        {
            InitializeComponent();
            Console.WriteLine("айди блока, у которого открыт полный лог:" + lb.LBId);
        }

    }
}
