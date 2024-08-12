﻿using System;
using System.Collections;
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
        LBAddConnect lBAddConnect = new LBAddConnect();
        public ObservableCollection<Dump> Dumps { get; set; }
        public WindowLBDumps(LB inputLB, int index, MainWindow MW)
        {
            InitializeComponent();
            currentLB = inputLB;
            LBName.Content = currentLB.LBName;
            LBIpPort.Content = currentLB.LBIpString + ":" + currentLB.LBPort.ToString();
            LBId.Content = currentLB.LBId;
            LBStatus.Content = currentLB.LBStatus;
            LBLastChange.Content = currentLB.LBLastChange;
            Title = currentLB.LBName;
/*
            lBAddConnect.GetDumpFromLBToSQLV7(inputLB, index, MW);*/

            Dumps = new ObservableCollection<Dump>(lBAddConnect.GetAllDumps(currentLB.LBId));
            DumpList.ItemsSource = Dumps;


        }


        private void DumpRowClick(object sender, RoutedEventArgs e)
        {

            StackPanel button = (StackPanel)sender;
            int id = Int32.Parse(button.Tag.ToString());

            var dumpview = new WindowDump(id, currentLB.LBId);
            dumpview.Owner = this;
            dumpview.Show();
        }

        private void OpenAllLogsButton_Click(object sender, RoutedEventArgs e)
        {
/*            Button button = (Button)sender;
            int tempId = Int32.Parse(button.Tag.ToString());*/
            WindowFullLog windowFullLog = new WindowFullLog(currentLB);
            windowFullLog.Show();

        }




    }
}
