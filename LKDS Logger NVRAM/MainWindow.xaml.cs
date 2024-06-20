﻿using System;
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

        public MainWindow()
        {
            InitializeComponent();
            LKDSFramework.DriverV7Net driverV7Net = new LKDSFramework.DriverV7Net();
            driverV7Net.Init();
            driverV7Net.Close();
            LB LBTemp = new LB();
            LBTemp.LBName = "qwertyuiopasdfgh";
            LBTemp.LBKey= "qwerty123";
            LBTemp.LBId = 55555;
            LBTemp.LBIpString = "127.123.213.121";
            LBTemp.LBPort = 12345;
            LBTemp.LBStatus = "отвечает";
            LBTemp.LBLastChange = "01.01.2003 12:49";

            LBs = new ObservableCollection<LB> { LBTemp };
            LBList.ItemsSource = LBs;
            Console.WriteLine(LBs[0].LBName);
            Image UICross = (Image)this.FindName("UICross");
            if (UICross != null)
            {
                Console.WriteLine("крестик найден");
                // Теперь вы можете работать с элементом UICross
            } else
            {
                Console.WriteLine("крестик не найден");
            }

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
            LBs.RemoveAt(flag); // крашится из-за того, что внутренний айди у лб не меняется, но, при удалении лб с меньшим айди, он перестает соответствовать его месту в коллекции. Пойду чаю бахну пока
        }

        
        private void LBRedact(int i)
        {

        }
        // 0 - имя, 1 - айди, 2 - ключ, 3 - айпи, 4 - порт
        public List<string> LBInputDataCheckIp(List<string> input)
        {
            List<string> ErrorList = new List<string>();

            if (input[0] == "" || input[1] == "" || input[2] == "" || input[3] == "" || input[4] == "")
            {
                ErrorList.Add("Присутствуют пустые поля");
                Console.WriteLine("присутствуют пустые поля");
                return ErrorList;
            }

            bool flag = true;
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


        private void LBRowClick(object sender, RoutedEventArgs e)
        {
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

            MessageBox.Show("Кнопка настроек нажата");
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
                UniversalKeyError.Content = "";
                UsingUniversalKey = true;
            }
        }

        
    }
}