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
using System.Threading;
using System.Globalization;
using Microsoft.SqlServer.Server;
using System.Data.Entity.Spatial;
using System.Windows.Threading;
using System.Reflection;

namespace LKDS_Logger_NVRAM
{   
    public partial class MainWindow : Window
    {
        public string filePath = "lbs.xml";
        public bool WindowsClosing = false;
        public string timeCheck;
        public bool InputsClear = false;
        public bool LBIsRedacted = false;
        public bool UsingUniversalKey = false;
        public string UniversalKey = "12345678";
        public static Mutex mutexMW = new Mutex(false, "Global\\MutexExample");
        public DateTime TimeStart;
        public int TimeInterval = 0;
        public DateTime TimeCheck;
        public int WaitToDetached = 0;
        private int TimeFromStartSub = 0;
        public ObservableCollection<LB> LBs { get; set; }
        List<string> LBTempWhileRedacting = new List<string>() { "", "", "", "", ""};
        LBAddConnect lBAddConnect = new LBAddConnect();
        public List<LB> LBListForDetached;
        private CancellationTokenSource cancellationTokenSource;
        DetachedAsks DA;
        private WindowState prevState;
        public MainWindow()
        {
            InitializeComponent();
            LKDSFramework.DriverV7Net driverV7Net = new LKDSFramework.DriverV7Net();

            if (!File.Exists("LBDumps.db3")){
                lBAddConnect.DBInitiate();
            }



            if (!string.IsNullOrEmpty(App.CommandLineArgument))
            {
                Console.WriteLine($"Аргумент командной строки: {App.CommandLineArgument}");
                List<LB> StartupLBs = GetStartupLBs(App.CommandLineArgument);
            }
            else
            {
                Console.WriteLine("Аргумент командной строки не указан.");
            }



            LBs = new ObservableCollection<LB>(lBAddConnect.AllLBIdFromSQL());
            LBList.ItemsSource = LBs;


            GetSettingsFromSQL(false);
            this.StateChanged += Window_StateChanged;
            this.TaskbarIcon.TrayLeftMouseDown += TaskbarIcon_TrayLeftMouseDown;

            Loaded += MainWindow_Loaded;

        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new System.Action(() =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                LBListForDetached = new List<LB>(LBs);
                DA = new DetachedAsks(mutexMW, 0, TimeInterval, new List<LB>(LBs), this, cancellationTokenSource.Token);
            }), DispatcherPriority.Background);
        }


        private List<LB> GetStartupLBs(string path)
        {
            List<LB> lBs = new List<LB>();

            // дописать функционал получения блока из файла, формат файла надо спрашивать

            return lBs;
        }

        private List<Dictionary<string, object>> GetSettingsFromSQL(bool type) // true - запись во все поля, false - возвращение данных в виде словаря
        {

            List<Dictionary<string, object>> settingsDict = lBAddConnect.GetAllSettings();
            if (type == true)
            {
                return settingsDict;
            }
            else
            {
                UniversalKey = LBUniversalKey.Text = settingsDict[0]["universal_key"].ToString();
                CheckBoxUniversalKey.IsChecked = UsingUniversalKey = (bool)settingsDict[0]["uk_use"];
                CheckBoxAddWindowClosing.IsChecked = WindowsClosing = (bool)settingsDict[0]["window_close"];
                CheckBoxInputClearing.IsChecked = InputsClear = (bool)settingsDict[0]["fields_clear"];
                List<int> timeInterval = TimeForInterval(Int32.Parse(settingsDict[0]["interval"].ToString()));
                TimeInterval = Int32.Parse(settingsDict[0]["interval"].ToString());
                MinutesUpDown.Text = timeInterval[1].ToString();
                HoursUpDown.Text = timeInterval[0].ToString();
                LBCheckStart.Value = DateTime.ParseExact(settingsDict[0]["dumping_start"].ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                LBTimeCheck.Value = DateTime.ParseExact(settingsDict[0]["changes_detect"].ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                timeCheck = LBTimeCheck.Value.ToString();
                PCIdentific.Text = settingsDict[0]["identific"].ToString();
                Console.WriteLine("ИДЕНТИФИК: " + settingsDict[0]["identific"].ToString());

                foreach (LB i in LBs)
                {
/*                    if (i.LBLastChange != "")
                    {*/
                        if (DateTime.ParseExact(i.LBLastChange, "dd-MM-yyyy HH:mm:ss", null) > TimeCheck)
                        {
                            i.LBColor = "LightPink";

                        }
                        else
                        {
                            i.LBColor = "White";

                        }
                    /*}*/
                }

                return settingsDict;
            }

        }

        private List<int> TimeForInterval(int seconds)
        {

            List<int> time = new List<int>();
            time.Add(seconds / 3600);
            time.Add((seconds % 3600)/60);
            return time;
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
            LBListForDetached.RemoveAt(flag);
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


            if (input[1].Length > 10)
            {
                ErrorList.Add("Длина айди");
            } else
            {
                for (int i = 0; i < LBs.Count; i++)
                {
                    if (LBs[i].LBId == Int32.Parse(input[1]))
                    {
                        ErrorList.Add("Такой ЛБ существует");
                        break;
                    }
                }
            }




            return ErrorList;
        }


        private void LBRowClick(object sender, RoutedEventArgs e)
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
            
            var windowLBDumps = new WindowLBDumps(LBs[flag], flag, this);

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

        private void ApplyLBButton_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            if (TimeFromStartSub < 0)
            {
                TimeFromStartSub = TimeFromStartSub * -1;
            }
            DA = new DetachedAsks(mutexMW, TimeFromStartSub, TimeInterval, new List<LB>(LBs), this, cancellationTokenSource.Token);
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
            Console.WriteLine("интервал в секундах: " + TimeInterval);

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
                TimeInterval = 0;
                TimeInterval += Int32.Parse(MinutesUpDown.Text) * 60;
                TimeStart = DateTimeCheckStart.Value;
                TimeInterval += Int32.Parse(HoursUpDown.Text) * 3600;
                TimeCheck = DateTimeCheckAfter.Value;

                DateTime end = DateTime.Now;
                int TimeFromStart = (int)((end - TimeStart).TotalSeconds);
                if(TimeInterval == 0)
                {
                    TimeInterval = 3600;
                }
                TimeFromStartSub = TimeFromStart % TimeInterval;
                Console.WriteLine(TimeFromStart + " " + TimeFromStartSub);
                WaitToDetached = TimeFromStartSub;

                cancellationTokenSource.Cancel();
                cancellationTokenSource = new CancellationTokenSource();
                if (TimeFromStartSub < 0) 
                {
                    TimeFromStartSub = TimeFromStartSub * -1;
                }
                DA = new DetachedAsks(mutexMW, TimeFromStartSub, TimeInterval, new List<LB>(LBs), this, cancellationTokenSource.Token);
                List<Dictionary<string, object>> tempData = GetSettingsFromSQL(true);

                lBAddConnect.UpdateSettings(tempData[0]["universal_key"].ToString(), (bool)tempData[0]["uk_use"], (bool)CheckBoxInputClearing.IsChecked, (bool)CheckBoxAddWindowClosing.IsChecked, TimeInterval,DateTimeCheckAfter.Value.ToString("yyyy-MM-dd HH:mm:ss"), DateTimeCheckStart.Value.ToString("yyyy-MM-dd HH:mm:ss"), Identific);
                SettingsErrorLabel.Content = "Применено";
                foreach (LB i in LBs)
                {
                    /*if (i.LBLastChange != "")
                    {*/
                        if (DateTime.ParseExact(i.LBLastChange, "dd-MM-yyyy HH:mm:ss", null) > TimeCheck)
                        {
                            i.LBColor = "LightPink";

                        }
                        else
                        {
                            i.LBColor = "White";

                        }

                    /*}*/
                }
            }
        }

        private void EditLBWindow_Closed(object sender, System.EventArgs e)
        {
            LBIsRedacted = false;
        }



        private void UniversalKeyButtonClick(object sender, RoutedEventArgs e)
        {
            if (LBUniversalKey.Text.Length < 6)
            {
                UniversalKeyError.Content = "Ключ слишком короткий";
                UsingUniversalKey = false;
                CheckBoxUniversalKey.IsChecked = false;
            } else
            {
                UniversalKey = LBUniversalKey.Text;
                UniversalKeyError.Content = "Ключ задан";
                UsingUniversalKey = true;
                CheckBoxUniversalKey.IsChecked = true;
                List<Dictionary<string, object>> tempData = GetSettingsFromSQL(true);

                lBAddConnect.UpdateSettings(UniversalKey, UsingUniversalKey, (bool)tempData[0]["fields_clear"], (bool)tempData[0]["window_close"], Int32.Parse(tempData[0]["interval"].ToString()), tempData[0]["dumping_start"].ToString(), tempData[0]["changes_detect"].ToString(), Int32.Parse(tempData[0]["identific"].ToString()));






            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
            else
            {
                prevState = WindowState;
            }
        }

        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = prevState;
            this.Activate(); // Активирует окно, чтобы оно было поверх других окон
        }

    }
    
}

