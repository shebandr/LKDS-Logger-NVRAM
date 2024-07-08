﻿using System;
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
        private int bytesCount = 180;
        private LBAddConnect lbAddConnect = new LBAddConnect();
        public ObservableCollection<ByteFromDump> byteFromDumpsCollection{ get; set; }
        public WindowDump(int idDump, int idLB)
        {
            InitializeComponent();
            Console.WriteLine("Открыт " + idDump + " дамп");
            


            List<string> lines = new List<string>
        {
            "версия микропрограммы",
            "версия микропрограммы",
            "Режим работы OUT1",
            "Количество реверса дверей",
            "Код регулир/режим CAN POWER",
            "Отображение USER1(In/Out)",
            "Отображение USER2(In/Out)",
            "Отображение USER3(In/Out)",
            "Отображение USER4(In/Out)",
            "Кол-во этажей",
            "Тест тракта ГГС",
            "Отображение ABL",
            "Признак групповой работы",
            "Звуковое оповещение",
            "Резерв",
            "Резерв",
            "Резерв",
            "Разрешение использования таблицы NVRAM",
            "Режим ТО",
            "Рестарт программы",
            "Многократный реверс дверей",
            "Проникновение в МП",
            "Дверь кабины открывается",
            "Зафиксировано движение в кабине лифта",
            "Открывание дверей кабины",
            "Открывание дверей шахты по УБ",
            "Отсутствует напряжение в цепи управления",
            "Зажата кнопка СТОП в кабине лифта",
            "Открыта дверь кабины лифта",
            "Разрыв цепи безопасности",
            "Не сработал датчик УБ",
            "КЗ цепи безопасности",
            "Вызов диспетчера",
            "Несанкционированное движение кабины",
            "Авария главного привода по УКСЛ",
            "Авария привода дверей",
            "Проникновение в шахту",
            "Неисправность УБ",
            "Автоматическая блокировка УПЛ/УЛЖ",
            "Открыто МП",
            "Перемычка пускателя, неиспр. оптосимистора",
            "Блокировка РД без РКД",
            "Кабина не пришла на этаж",
            "Не сработал датчик ДК",
            "Не закрыта дверь шахты по УБ",
            "Главный привод включен",
            "Бит пользователя",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "Состояние USER1",
            "Состояние USER2",
            "Состояние USER3",
            "Состояние USER4",
            "Резервное питание ЛБ",
            "Переменное напряжение в ЦБ",
            "Пожарная опасность",
            "Неисправна батарея питания",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "Нет документации",
            "Нет документации",
            "Бит разрешения включения лифта по запросу с линии",
            "Бит инверсии датчиков УБ для немостовой схемы",
            "Интерпретация входа ABL",
            "Откл лифта при вкл ЛБ",
            "Проникнование в шахту по ДК",
            "Резерв",
            "Резерв",
            "Резерв",
            "Разрешение формирования дополнительных состояний",
            "Резерв",
            "Резерв",
            "Использование внешнего УКСЛ",
            "Скорость обмена со станцией УКЛ/УЛ",
            "Режим отображения индикатора ДШ",
            "Скорость лифта",
            "Тип Датчика УКСЛ",
            "Режим ТО",
            "Рестарт программы",
            "Многократный реверс дверей",
            "Проникновение в МП",
            "Дверь кабины открывается",
            "Зафиксировано движение кабины лифта",
            "Открывание дверей кабины",
            "Открывание дверей шахты по УБ",
            "Отсутствует напряжение в цепи управления",
            "Зажата кнопка СТОП в кабине лифта",
            "Открыта дверь кабины лифта",
            "Разрыв цепи безопасности",
            "Не сработал датчик УБ",
            "КЗ цепи безопасности",
            "Вызов диспетчера",
            "Несанкционированное движение кабины",
            "Авария главного привода по УКСЛ",
            "Авария привода дверей",
            "Проникновение в шахту",
            "Неисправность УБ",
            "Автоматическая блокировка УПЛ/УЛЖ",
            "Открыто МП",
            "Перемычка пускателя, неиспр. оптосимистора",
            "Бловирока РД без РКД",
            "Кабина не пришла на этаж",
            "Не сработал датчик ДК",
            "Не закрыта дверь шахты по УБ",
            "Главный привод включен",
            "Бит пользователя",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "Состояние USER1",
            "Состояние USER2",
            "Состояние USER3",
            "Состояние USER4",
            "Резервное питание ЛБ",
            "Переменное напряжение в ЦБ",
            "Пожарная опасность",
            "Неисправна батеря питания",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ", 
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "Режим ТО",
            "Рестарт программы",
            "Многократный реверс дверей",
            "Проникновение в МП",
            "Дверь кабины открывается",
            "Зафиксировано движение кабины лифта",
            "Открывание дверей кабины",
            "Открывание дверей шахты по УБ",
            "Отсутствует напряжение в цепи управления",
            "Зажата кнопка СТОП в кабине лифта",
            "Открыта дверь кабины лифта",
            "Разрыв цепи безопасности",
            "Не сработал датчик УБ",
            "КЗ цепи безопасности",
            "Вызов диспетчера",
            "Несанкционированное движение кабины",
            "Авария главного привода по УКСЛ",
            "Авария привода дверей",
            "Проникновение в шахту",
            "Неисправность УБ",
            "Автоматическая блокировка УПЛ/УЛЖ",
            "Открыто МП",
            "Перемычка пускателя, неиспр. оптосимистора",
            "Бловирока РД без РКД",
            "Кабина не пришла на этаж",
            "Не сработал датчик ДК",
            "Не закрыта дверь шахты по УБ",
            "Главный привод включен",
            "Бит пользователя",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "Состояние USER1",
            "Состояние USER2",
            "Состояние USER3",
            "Состояние USER4",
            "Резервное питание ЛБ",
            "Переменное напряжение в ЦБ",
            "Пожарная опасность",
            "Неисправна батеря питания",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ",
            "РЕЗЕРВ"

        };

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
                string[] bytes = BytesToBits(AllDumps[idDump-1].Data.Split(' '));
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
                string[] bytesCurrent = BytesToBits(AllDumps[idDump-1].Data.Split(' '));
                string[] bytesPrev = BytesToBits(AllDumps[idDump-2].Data.Split(' '));
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
                        Console.WriteLine("поменялись данные с индексом " + i + " с " + bytesPrev[i] + " на " + bytesCurrent[i]);
                        byteFromDump.isChanged = true;
                    }
                    AllBytes.Add(byteFromDump);
                }
            }
            Title = AllDumps[idDump-1].TimeDate;
            int tempNVRAMNum = 0;
            for(int i = 1; i < 67; i++)
            {
                AllBytes[tempNVRAMNum].NVRAMAddres = i;
                tempNVRAMNum++;
            }
            tempNVRAMNum+=2;
            for (int i = 83; i < 195; i++)
            {
                AllBytes[tempNVRAMNum].NVRAMAddres = i;
                tempNVRAMNum++;
            }

            byteFromDumpsCollection = new ObservableCollection<ByteFromDump>(AllBytes);
            ByteDumpList.ItemsSource = byteFromDumpsCollection;
        }
        private string[] BytesToBits(string[] input)
        {
            int bytesCount = input.Length;
            string[] output = new string[bytesCount * 8];
            int f = 0;

            // Первые 18 байт заносятся в итоговый список в виде хекс данных
            for (int i = 0; i < 18; i++)
            {
                output[f] = input[i];
                f++;
            }

            // Обработка байтов с 18 по 23
            for (int i = 18; i < 24; i++)
            {
                List<string> bit = HexToBitList(input[i]);
                for (int q = 0; q < 8; q++)
                {
                    output[f] = bit[7-q];
                    f++;
                }
            }

            // Байты 24 и 25 заносятся в итоговый список в виде хекс данных
            output[f] = input[24];
            f++;
            output[f] = input[25];
            f++;

            // Обработка байтов с 26 по 39
            for (int i = 26; i < 40; i++)
            {
                List<string> bit = HexToBitList(input[i]);
                for (int q = 0; q < 8; q++)
                {
                    output[f] = bit[7 - q];
                    f++;
                }
            }

            return output;
        }

        public static List<string> HexToBitList(string hexString)
        {
            int hexValue = Convert.ToInt32(hexString, 16);

            List<string> bitStringList = new List<string>();

            for (int i = 7; i >= 0; i--)
            {
                bitStringList.Add(((hexValue >> i) & 1).ToString());
            }

            return bitStringList;
        }
    }
}
