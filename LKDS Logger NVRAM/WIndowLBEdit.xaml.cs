using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для WIndowLBEdit.xaml
    /// </summary>
    public partial class WIndowLBEdit : Window
    {
        private MainWindow WMG;
        private string TempIpString = "";
        private string TempPortString = "";
        private int LBConnectType = 0;
        private int StartId;
        private LBAddConnect lBAddConnect = new LBAddConnect();
        public WIndowLBEdit(MainWindow MW)
        {
            InitializeComponent();
            WMG = MW;
  
            buttonLBAdd.Click += AddLBButtonClick;
            buttonLBAdd.Content = "Добавить ЛБ";
            rb1.IsChecked = true;
            TempIpString = LBIPString.Text;
            TempPortString = LBPortString.Text;
            Console.WriteLine(WMG.UsingUniversalKey + " " + WMG.UniversalKey);
            if (WMG.UsingUniversalKey)
            {
                LBKey.Text = WMG.UniversalKey;
            }
        }

        public WIndowLBEdit(MainWindow MW, int LBIndex)
        {
            InitializeComponent();
            WMG = MW;
            buttonLBAdd.Click += LBRedactApplyButton_Click;
            buttonLBAdd.Content = "Применить";
            LBRedactButton_Click(LBIndex);
            
            if (WMG.LBs[LBIndex].LBIpString == "LKDSCloud")
            {
                rb0.IsChecked = true;
            }
            else
            {
                rb1.IsChecked = true;
            }
            
        }


        private void LBRedactButton_Click(int RedactingLB)
        {



            LBName.Text = WMG.LBs[RedactingLB].LBName;
            LBID.Text = WMG.LBs[RedactingLB].LBId.ToString();
            LBKey.Text = WMG.LBs[RedactingLB].LBKey;
            LBIPString.Text = WMG.LBs[RedactingLB].LBIpString;
            LBPortString.Text = WMG.LBs[RedactingLB].LBPort.ToString();
            buttonLBAdd.Tag = RedactingLB.ToString();
            TempIpString = WMG.LBs[RedactingLB].LBIpString;
            TempPortString = WMG.LBs[RedactingLB].LBPort.ToString();
            StartId = WMG.LBs[RedactingLB].LBId;
        }

        private void LBRedactApplyButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int RedactingLB = Int32.Parse(button.Tag.ToString());
            Console.WriteLine("редактируется: " + RedactingLB.ToString());


            List<string> input = new List<string>();
            input.Add(LBName.Text);
            input.Add(LBID.Text);
            input.Add(LBKey.Text);
            input.Add(LBIPString.Text);
            input.Add(LBPortString.Text);

            List<string> ErrorList;
            if (LBConnectType == 1)
            {
                ErrorList= WMG.LBInputDataCheckLKDSCloud(input);
            } else
            {
                ErrorList = WMG.LBInputDataCheckIp(input);
            }
            
            bool flag;

            if (ErrorList.Count > 0 && ErrorList[0]!= "Такой ЛБ существует") { flag = false; } else { flag = true; }
            foreach(string i in ErrorList)
            {
                Console.WriteLine(i);
            }



            if (flag)
            {
                LB LBTemp = new LB();

                if (LBConnectType == 0)
                {
                    LBTemp.LBIpString = LBIPString.Text;
                    LBTemp.LBPort = Int32.Parse(LBPortString.Text);

                }
                else
                {
                    LBTemp.LBIpString = "LKDSCloud";
                    LBTemp.LBPort = 0;
                }
                LBTemp.LBName = LBName.Text;
                LBTemp.LBId = Int32.Parse(LBID.Text);
                LBTemp.LBKey = LBKey.Text;
                LBTemp.LBStatus = WMG.LBs[RedactingLB].LBStatus;
                LBTemp.LBLastChange = WMG.LBs[RedactingLB].LBLastChange;
                LBTemp.LBLastDump = WMG.LBs[RedactingLB].LBLastDump;
                WMG.LBs[RedactingLB] = LBTemp;
                lBAddConnect.LBEditSQL(LBTemp, StartId);
                if (WMG.WindowsClosing == true)
                {
                    this.Close();
                }
                
            } else
            {
                if (ErrorList.Count() > 0)
                {

                    string tempError = "Ошибки с: ";
                    for (int i = 0; i < ErrorList.Count() - 1; i++)
                    {
                        if(ErrorList[i] != "Такой ЛБ существует")
                        {
                            tempError += ErrorList[i] + ", ";
                        }
                        
                    }
                    tempError += ErrorList[ErrorList.Count() - 1];
                    LBAddErrorLabel.Content = tempError;
                }
                else
                {
                    LBAddErrorLabel.Content = "Ошибка: такой ЛБ уже есть";
                }

            
        
            }

        }


        private void ButtonClickRedactCancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddLBButtonClick(object sender, RoutedEventArgs e)
        {
            // 0 - имя, 1 - айди, 2 - ключ, 3 - айпи, 4 - порт
            List<string> input = new List<string>();
            input.Add(LBName.Text);
            input.Add(LBID.Text);
            input.Add(LBKey.Text);
            input.Add(LBIPString.Text);
            input.Add(LBPortString.Text);

            List<string> ErrorList;
            if (LBConnectType == 1)
            {
                ErrorList = WMG.LBInputDataCheckLKDSCloud(input);
            }
            else
            {
                ErrorList = WMG.LBInputDataCheckIp(input);
            }
            bool flag;
            if (ErrorList.Count > 0) { flag = false; } else { flag = true; }
            LB LBDataTemp = new LB();


            if (flag)
            {

                LBDataTemp.LBName = input[0];
                LBDataTemp.LBId = Int32.Parse(input[1]);
                LBDataTemp.LBIpString = input[3];
                if(LBConnectType == 1)
                {
                    LBDataTemp.LBPort = 0;
                } else
                {
                    LBDataTemp.LBPort = Int32.Parse(input[4]);
                }
                LBDataTemp.LBKey = input[2];
                LBAddErrorLabel.Content = "";
                LBDataTemp.LBStatus = "не опрошен";
                LBDataTemp.LBLastChange = " - ";
                LBDataTemp.LBLastDump = " - ";
                WMG.LBs.Add(LBDataTemp);
                lBAddConnect.LBToSQL(LBDataTemp);
                Console.WriteLine("успешно добавлено: " + WMG.LBs[WMG.LBs.Count - 1].LBName + WMG.LBs[WMG.LBs.Count - 1].LBId + WMG.LBs[WMG.LBs.Count - 1].LBKey + WMG.LBs[WMG.LBs.Count - 1].LBIpString + WMG.LBs[WMG.LBs.Count - 1].LBPort + WMG.LBs[WMG.LBs.Count - 1].LBStatus + WMG.LBs[WMG.LBs.Count - 1].LBLastChange);
                if (WMG.InputsClear)
                {
                    LBName.Text = "";
                    LBID.Text = "";
                    LBKey.Text = "";
                    LBIPString.Text = "";
                    LBPortString.Text = "";
                }
                if (WMG.WindowsClosing)
                {
                    if (WMG.WindowsClosing == true)
                    {
                        this.Close();
                    }
                }
            }
            else
            {
                if (ErrorList.Count() > 0)
                {

                    string tempError = "Ошибки с: ";
                    for (int i = 0; i < ErrorList.Count() - 1; i++)
                    {
                        tempError += ErrorList[i] + ", ";
                    }
                    tempError += ErrorList[ErrorList.Count() - 1];
                    LBAddErrorLabel.Content = tempError;
                }
                else
                {
                    LBAddErrorLabel.Content = "Ошибка: такой ЛБ уже есть";
                }

            }
        }
        void LBTypeConnect(object sender, EventArgs e)
        {
            RadioButton li = (sender as RadioButton);
            string TempRBD = li.Content.ToString();
            switch (TempRBD)
            {
                case "Через LKDSCloud по ключу":
                    LBIPString.IsReadOnly = true;
                    LBPortString.IsReadOnly = true;
                    TempIpString = LBIPString.Text;
                    TempPortString = LBPortString.Text;
                    LBIPString.Text = "LKDSCloud";
                    LBPortString.Text = "";
                    LBConnectType = 1;
                    break;
                case "По IP сети":
                    LBIPString.IsReadOnly = false;
                    LBPortString.IsReadOnly = false;
                    LBIPString.Text = TempIpString;
                    LBPortString.Text = TempPortString;
                    LBConnectType = 0;
                    break;
            }
        }


    }
}
