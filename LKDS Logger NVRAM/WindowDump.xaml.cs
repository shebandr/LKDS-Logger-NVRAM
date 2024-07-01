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
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Shell;

namespace LKDS_Logger_NVRAM
{
    /// <summary>
    /// Логика взаимодействия для WindowDump.xaml
    /// </summary>
    /// 
    public partial class WindowDump : Window
    {
        private int bytesCount = 255;
        private LBAddConnect lbAddConnect = new LBAddConnect();
        public ObservableCollection<ByteFromDump> byteFromDumpsCollection{ get; set; }
        public WindowDump(int idDump, int idLB)
        {
            InitializeComponent();
            Console.WriteLine("Открыт " + idDump + " дамп");
            string filePath = "data.txt";

            // Создаем список для хранения строк
            List<string> lines = new List<string>();

            try
            {
                // Читаем все строки из файла и добавляем их в список
                lines.AddRange(File.ReadAllLines(filePath));
            }
            catch (Exception ex)
            {
                // Обрабатываем возможные ошибки при чтении файла
                Console.WriteLine("Ошибка при чтении файла: " + ex.Message);
            }
            if (lines.Count < bytesCount)
            {
                for(int i = 0; i < bytesCount; i++)
                {
                    lines.Add("???");
                }
            }
            List<Dump> AllDumps = lbAddConnect.GetAllDumps(idLB);
            
            List<ByteFromDump> AllBytes = new List<ByteFromDump>();

            if (AllDumps.Count == 1)
            {
                string[] bytes = AllDumps[0].Data.Split(' ');
                for (int i = 0; i < bytesCount; i++)
                {
                    ByteFromDump byteFromDump = new ByteFromDump();
                    byteFromDump.id = i+1;
                    byteFromDump.name = lines[i];
                    byteFromDump.data = bytes[i];
                    byteFromDump.isChanged = false;
                    AllBytes.Add(byteFromDump);
                }
            }
            else
            {
                string[] bytesCurrent = AllDumps[AllDumps.Count - 1].Data.Split(' ');
                string[] bytesPrev = AllDumps[AllDumps.Count - 2].Data.Split(' ');
                for (int i = 0; i < bytesCount; i++)
                {
                    ByteFromDump byteFromDump = new ByteFromDump();
                    byteFromDump.id = i+1;
                    byteFromDump.name = lines[i];
                    byteFromDump.data = bytesCurrent[i];
                    if (bytesCurrent[i] == bytesPrev[i])
                    {
                        byteFromDump.isChanged = false;
                    } else
                    {
                        byteFromDump.isChanged = true;
                    }
                    AllBytes.Add(byteFromDump);
                }
            }
            byteFromDumpsCollection = new ObservableCollection<ByteFromDump>(AllBytes);
            ByteDumpList.ItemsSource = byteFromDumpsCollection;
        }

    }
}
