using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
        public ObservableCollection<string> DumpsTIme { get; set; } = new ObservableCollection<string>();
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

            if (Dumps.Count > 0)
            {
                //вычисление времени всех логов, чтобы задать в инпуты дейттам
                string startDateTime = Dumps[0].TimeDate.ToString();
                string endDateTime = Dumps[Dumps.Count - 1].TimeDate.ToString();

                TimeStart.Value = DateTime.ParseExact(startDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                TimeStop.Value = DateTime.ParseExact(endDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                //сбор коллекции, которая будет создавать горизонтальную часть таблицы
                foreach (Dump dump in Dumps) 
                {
                    string tempTimeDate = dump.TimeDate.ToString();
                    string[] tempTimeDateList = tempTimeDate.Split(' ');
                    DumpsTIme.Add(tempTimeDateList[0] + " \n" + tempTimeDateList[1]);
                    
                }
                Console.WriteLine(DumpsTIme[0]);
            }

        }

        private void ButtonSettingsApply_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
