using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics.Eventing.Reader;

namespace LKDS_Logger_NVRAM
{   
    public partial class MainWindow : Window
    {
        public string filePath = "lbs.xml";
        public bool WindowsClosing = false;
        public bool InputsClear = false;
        public bool LBIsRedacted = false;
        public bool UsingUniversalKey = false;
        public string UniversalKey = "12345678";
        public ObservableCollection<LB> LBs { get; set; }
        List<string> LBTempWhileRedacting = new List<string>() { "", "", "", "", ""};
        LBAddConnect lBAddConnect = new LBAddConnect();
        public MainWindow()
        {
            InitializeComponent();
            LKDSFramework.DriverV7Net driverV7Net = new LKDSFramework.DriverV7Net();
            /*LB LBTemp = new LB();
            LBTemp.LBName = "123";
            LBTemp.LBKey= "qwerty1234";
            LBTemp.LBId = 51191;
            LBTemp.LBIpString = "LKDSCloud";
            LBTemp.LBPort = 0;
            LBTemp.LBStatus = "отвечает";
            LBTemp.LBLastChange = "01.01.2003 12:49";
*/
            
            /*lBAddConnect.DBInitiate()*/
            /*            ;
                        LB tempLB = new LB();
                        tempLB = lBAddConnect.LBFromSQL(51191);*/
            if (!File.Exists("LBDumps.db3")){
                lBAddConnect.DBInitiate();
            }
            LBs = new ObservableCollection<LB>(lBAddConnect.AllLBIdFromSQL());
            LBList.ItemsSource = LBs;

            
        }


        private void LBRedactButton_Click(object sender, RoutedEventArgs e)
        {
            if (LBIsRedacted == true)
            {
                MessageBox.Show("Сначала завершите редактирование лифтового блока");
                return;
            }
            LBIsRedacted = true;
            Button button = (Button)sender;
            int tempId = Int32.Parse(button.Tag.ToString());
            int flag = 0;
            for (int i = 0; i < LBs.Count(); i++)
            {
                if (LBs[i].LBId == tempId)
                {
                    flag = i;
                    break;
                }
            }
            WIndowLBEdit WindowEdit = new WIndowLBEdit(this, flag);
            WindowEdit.Closed += EditLBWindow_Closed;
            WindowEdit.Owner = this;
            WindowEdit.Show();


        }

        private void InputsClearcheckBox_Checked(object sender, RoutedEventArgs e)
        {
            InputsClear = true;
            Console.WriteLine("чекбокс отмечен");
        }

        private void InputsClearcheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            InputsClear = false;
            Console.WriteLine("чекбокс не отмечен");
        }

        private void WindowClosingcheckBox_Checked(object sender, RoutedEventArgs e)
        {
            WindowsClosing = true;
            Console.WriteLine("чекбокс отмечен");
        }

        private void WindowClosingcheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            WindowsClosing = false;
            Console.WriteLine("чекбокс не отмечен");
        }

        private void UniversalKeyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UsingUniversalKey = true;
            Console.WriteLine("чекбокс отмечен");
        }

        private void UniversalKeyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UsingUniversalKey = false;
            Console.WriteLine("чекбокс не отмечен");
        }
        private void LBDelButton_Click(object sender, RoutedEventArgs e)
        {
            if(LBIsRedacted == true)
            {
                MessageBox.Show("Сначала завершите редактирование лифтового блока");
                return;
            }
            Button button = (Button)sender;
            int tempId = Int32.Parse(button.Tag.ToString());
            int flag = 0;
            for(int i = 0; i < LBs.Count(); i++)
            {
                if (LBs[i].LBId == tempId)
                {
                    flag = i;
                    break;
                }
            }
            lBAddConnect.LBDelSQL(LBs[flag]);
            LBs.RemoveAt(flag); // крашится из-за того, что внутренний айди у лб не меняется, но, при удалении лб с меньшим айди, он перестает соответствовать его месту в коллекции. Пойду чаю бахну пока
            
        }

        
        private void LBRedact(int i)
        {

        }
        // 0 - имя, 1 - айди, 2 - ключ, 3 - айпи, 4 - порт
        public List<string> LBInputDataCheckIp(List<string> input)
        {
            List<string> ErrorList = new List<string>();

            bool flag = true;
            if (input[0] == "" || input[1] == "" || input[2] == "" || input[3] == "" || input[4] == "")
            {
                ErrorList.Add("Присутствуют пустые поля");
                Console.WriteLine("присутствуют пустые поля");
                return ErrorList;
            }
            if (input[1].Length > 9)
            {
                ErrorList.Add("Слишком большой идентификатор");

            }
            else
            {
                try
                {
                    Int32.Parse(input[1]);
                }
                catch (FormatException)
                {
                    flag = false;
                    Console.WriteLine("невозможно перевести");
                    ErrorList.Add("Идентификатор");
                    //   MessageBox.Show("Неправильный идентификатор Лифтового блока");
                }
            }


            if (!flag)
            {
                ErrorList.Add("Идентификатор невозможен");
                Console.WriteLine("айди <0");
                //   MessageBox.Show("отрицательный идентификатор Лифтового блока");

            }


            // 0 - имя, 1 - айди, 2 - ключ, 3 - айпи, 4 - порт

            if (input[2].Length < 6)
            {
                ErrorList.Add("Длина ключа");
            }




            Regex r = new Regex(@"^((1\d\d|2([0-4]\d|5[0-5])|\d\d?)\.?){4}$");
            if (!r.IsMatch(input[3]))
            {
                Console.WriteLine("проблемы с IP");
                ErrorList.Add("IP");
            }

            if (input[4].Length > 6)
            {
                ErrorList.Add("Порт");
            }
            else
            {
                try
                {
                    Int32.Parse(input[4]);
                }
                catch (FormatException)
                {
                    ErrorList.Add("Порт");
                    Console.WriteLine("невозможно перевести порт");
                    //   MessageBox.Show("Неправильный порт Лифтового блока");
                }
            }


            if (ErrorList.Count != 0 && ErrorList[ErrorList.Count - 1] != "Порт")
            {
                if (Int32.Parse(input[4])> 65536 || Int32.Parse(input[4]) < 1024)
                {
                    ErrorList.Add("Порт");
                }
            }


            for (int i = 0; i < LBs.Count; i++)
            {
                if (LBs[i].LBId == Int32.Parse(input[1]))
                {
                    ErrorList.Add("Такой ЛБ существует");
                    break;
                }
            }

            return ErrorList;
        }
        // 0 - имя, 1 - айди, 2 - ключ, 3 - айпи, 4 - порт
        public List<string> LBInputDataCheckLKDSCloud(List<string> input)
        {
            List<string> ErrorList = new List<string>();

            if (input[0] == "" || input[1] == "" || input[2] == "" )
            {
                ErrorList.Add("Присутствуют пустые поля");
                Console.WriteLine("присутствуют пустые поля");
                return ErrorList;
            }
            bool flag = true;
            if (input[1].Length >9)
            {
                ErrorList.Add("Слишком большой идентификатор");
                
            } else
            {
                try
                {
                    Int32.Parse(input[1]);
                }
                catch (Exception)
                {
                    flag = false;
                    Console.WriteLine("невозможно перевести");
                    ErrorList.Add("Идентификатор");
                    //   MessageBox.Show("Неправильный идентификатор Лифтового блока");
                }
            }

            

            if (!flag)
            {
                ErrorList.Add("Идентификатор невозможен");
                Console.WriteLine("айди <0");
                //   MessageBox.Show("отрицательный идентификатор Лифтового блока");

            }


            // 0 - имя, 1 - айди, 2 - ключ, 3 - айпи, 4 - порт

            if (input[2].Length < 6)
            {
                ErrorList.Add("Длина ключа");
            }

            



            for (int i = 0; i < LBs.Count; i++)
            {
                if (LBs[i].LBId == Int32.Parse(input[1]))
                {
                    ErrorList.Add("Такой ЛБ существует");
                    break;
                }
            }

            return ErrorList;
        }


        private async void LBRowClick(object sender, RoutedEventArgs e)
        {


            
/*            List<string> temp = lBAddConnect.GetDump(LBs[0]);
            Console.WriteLine("принятые данные в мв: " + string.Join(" ", temp));*/



            StackPanel button = (StackPanel)sender;
            Console.Write(button.Tag);
            int tempId = Int32.Parse(button.Tag.ToString());
            int flag = 0;
            for (int i = 0; i < LBs.Count(); i++)
            {
                if (LBs[i].LBId == tempId)
                {
                    flag = i;
                    break;
                }
            }
            var windowLBDumps = new WindowLBDumps(LBs[flag]);

            windowLBDumps.Owner = this;
            windowLBDumps.Show();
        }

        

        private void AddLBButton_Click(object sender, RoutedEventArgs e)
        {
            if (LBIsRedacted == true)
            {
                MessageBox.Show("Сначала завершите редактирование лифтового блока");
                return;
            }
            LBIsRedacted = true;
            WIndowLBEdit WindowEdit = new WIndowLBEdit(this);
            WindowEdit.Closed += EditLBWindow_Closed;
            WindowEdit.Owner = this;
            WindowEdit.Show();

        }



        private void SettingsApplyButtonClick(object sender, RoutedEventArgs e)
        {
            List<string> ErrorList = new List<string>();
            int Hours = -1;
            if (Int32.TryParse(HoursUpDown.Text, out Hours))
            {

            }
            else
            {
                ErrorList.Add("Часы");
            }
            int Minutes = -1;
            if (Int32.TryParse(MinutesUpDown.Text, out Minutes))
            {


            }
            else
            {
                ErrorList.Add("Минуты");

            }

            DateTime? DateTimeCheckAfter = LBTimeCheck.Value;
            if (DateTimeCheckAfter.HasValue)
            {

                Console.WriteLine("Selected DateTime: " + DateTimeCheckAfter.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else
            {

                ErrorList.Add("Время подсветки");
            }

            DateTime? DateTimeCheckStart = LBCheckStart.Value;
            if (DateTimeCheckStart.HasValue)
            {

                Console.WriteLine("Selected DateTime: " + DateTimeCheckStart.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else
            {

                ErrorList.Add("Пустое начало опроса");
            }
            int Identific = -1;
            if (Int32.TryParse(PCIdentific.Text, out Identific))
            {


            }
            else
            {
                ErrorList.Add("Идентифик.");

            }
            if (ErrorList.Count() > 0)
            {

                string tempError = "Ошибки с: ";
                for (int i = 0; i < ErrorList.Count() - 1; i++)
                {
                    if (ErrorList[i] != "Такой ЛБ существует")
                    {
                        tempError += ErrorList[i] + ", ";
                    }

                }
                tempError += ErrorList[ErrorList.Count() - 1];
                SettingsErrorLabel.Content = tempError;
            } else
            {
                SettingsErrorLabel.Content = "Применено";
                // реализовать тут засовывание переменных в глобальные настройки
            }
        }

        private void EditLBWindow_Closed(object sender, System.EventArgs e)
        {
            LBIsRedacted = false;
        }



        private void UniversalKeyButtonClick(object sender, RoutedEventArgs e)
        {
            UniversalKey = LBUniversalKey.Text;
            Console.WriteLine(UniversalKey);
            if (UniversalKey.Length < 6)
            {
                UniversalKeyError.Content = "Ключ слишком короткий";
                UsingUniversalKey = false;
            } else
            {
                UniversalKeyError.Content = "Ключ задан";
                UsingUniversalKey = true;
            }
        }



    }
}
