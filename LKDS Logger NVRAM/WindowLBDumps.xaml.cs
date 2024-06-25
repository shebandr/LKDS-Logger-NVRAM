using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    /// Логика взаимодействия для WindowLBDumps.xaml
    /// </summary>
    public partial class WindowLBDumps : Window
    {
        LB currentLB;
        public ObservableCollection<Dump> Dumps { get; set; }
        public WindowLBDumps(LB inputLB)
        {
            InitializeComponent();
            currentLB = inputLB;
            LBName.Content = currentLB.LBName;
            LBIpPort.Content = currentLB.LBIpString + ":" + currentLB.LBPort.ToString();
            LBId.Content = currentLB.LBId;
            LBStatus.Content = currentLB.LBStatus;
            LBLastChange.Content = currentLB.LBLastChange;
            Title = currentLB.LBName;

            
  /*          string[] allfiles = new string[2];
            allfiles[0] = "q";
            allfiles[1] = "w";
  */          

            Dumps = new ObservableCollection<Dump> { };
/*            foreach (string filename in allfiles)
            {
                Dump DumpTemp = new Dump();
                DumpTemp.LBId = currentLB.LBId;
                DumpTemp.IsChanged = false;
                string[] ST = filename.Split('\\');
                
                DumpTemp.TimeDate = ST[2].Substring(0, ST[2].Length - 4);
                Console.WriteLine(ST[2].Substring(0, ST[2].Length - 4));
                Dumps.Add(DumpTemp);
                Console.WriteLine(filename);
            }
*/
            DumpList.ItemsSource = Dumps;


        }
    }
}
