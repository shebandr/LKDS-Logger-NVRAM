using LKDSFramework.Packs;
using LKDSFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LKDSFramework.Packs.DataDirect;
using System.Windows.Media;
using System.Threading;
using System.Data.SQLite;
using System.Data.Common;
using System.Data;

namespace LKDS_Logger_NVRAM
{
    internal class LBAddConnect
    {
        public LBAddConnect() { }
        public void StartService()
        {
            Console.WriteLine("подключение фреймворка");
            DriverV7Net driver = new DriverV7Net();
            driver.OnDevChange = delegate (DeviceV7 dev)
            {
                Console.WriteLine($"Устройство {dev} {(dev.isOnline ? "Вышло на связь" : "Ушло со связи")}");
                
                /*                PackV7RestartAsk restartAsk = new PackV7RestartAsk();

                                restartAsk.UnitID = 51191;
                                Console.WriteLine("до отправки" + restartAsk.State.ToString() + " " + restartAsk.Data.ToString() + " " + restartAsk.Type.ToString());
                                if ((restartAsk.PackID = driver.SendPack(restartAsk)) == 0)
                                {
                                    Console.WriteLine("Не получилось поставить в очередь пакет");

                                }
                                Console.WriteLine("после отправки" + restartAsk.State.ToString() + " " + restartAsk.Data.ToString() + " " + restartAsk.Type.ToString());*/
            };
            /*driver.OnSubChange = delegate (SubDeviceV7 sub)
            {
                Console.WriteLine($"Устройство CAN {sub} в {sub.Parrent} {(sub.isOnline ? "Вышло на связь" : "Ушло со связи")}");
            };*/
            driver.OnReceiveData = delegate (PackV7 pack)
            {
                if (pack is LKDSFramework.Packs.DataDirect.PackV7NVRAMAns)
                {
                    LKDSFramework.Packs.DataDirect.PackV7NVRAMAns dump = pack as LKDSFramework.Packs.DataDirect.PackV7NVRAMAns;
                    Console.Write("данные получены: " + dump.Data.Count() + " ");
                    foreach (var i in dump.Data)
                    {
                        Console.Write(i + " ");
                    }
                    Console.WriteLine();
                }

            };

            if (driver.Init())
            {
                DeviceV7 dev = new DeviceV7()
                {
                    IP = DeviceV7.CloudIP,
                    Port = DeviceV7.CloudPort,
                    UnitID = 51191,
                    Pass = "qwerty1234"
                };
                driver.AddDevice(ref dev);
                for(int i = 0; i < 65536; i += 255) {
                    PackV7NVRAMAsk nvramAsk = new PackV7NVRAMAsk();
                    Union16 union16 = new Union16();
                    union16.Value = (short)i;
                    nvramAsk.Address = union16;
                    nvramAsk.NVRAMLen = 255;
                    nvramAsk.isWriteNVRAM = false;
                    nvramAsk.UnitID = 51191;
                    if ((nvramAsk.PackID = driver.SendPack(nvramAsk)) == 0)
                    {
                        Console.WriteLine("Не получилось поставить в очередь пакет");

                    }
                }


                 driver.Close();
            }
        }


        private static ManualResetEvent doneEvent = new ManualResetEvent(false);
        public async Task<List<string>> GetDump(LB LBAsked)
        {
            int DumpSizeButes = 64;
            int PacketSize = 64;
            int BytesCount = 0;
            int StartByte = 0;
            List<byte> dumpBytes = new List<byte>();
            int flag = 0;
            List<string> dumpStr = new List<string>();
            DriverV7Net driver = new DriverV7Net();
            var tcs = new TaskCompletionSource<List<string>>();

            driver.OnReceiveData =  (PackV7 pack) =>
            {
                if (pack is LKDSFramework.Packs.DataDirect.PackV7NVRAMAns)
                {
                    LKDSFramework.Packs.DataDirect.PackV7NVRAMAns dump = pack as LKDSFramework.Packs.DataDirect.PackV7NVRAMAns;
                    Console.Write("данные получены: " + dump.Data.Count() + " ");
                    foreach (var i in dump.Data)
                    {
                        if (flag > 4)
                        {
                            dumpBytes.Add(i);

                        }
                        flag++;
                        Console.Write(i + " ");
                        BytesCount++;

                    }
                    Console.WriteLine();
                
                Console.WriteLine("размеры дампа до перевода и после: ");
                Console.WriteLine(dumpBytes.Count);
                string tempStrDump = BitConverter.ToString(dumpBytes.ToArray()).Replace("-", string.Empty);
                Console.WriteLine(tempStrDump.Length);
               
                Console.WriteLine(tempStrDump);
                for (int i = 0; i < dumpBytes.Count; i++)
                {
                    dumpStr.Add(tempStrDump[i * 2].ToString() + tempStrDump[i * 2 + 1].ToString());
                    Console.WriteLine(i.ToString() + ": " + dumpBytes[i].ToString() + " " + tempStrDump[i * 2].ToString() + tempStrDump[i * 2 + 1].ToString());
                }

                driver.Close();
                    Console.WriteLine("конец асинхронной работы");
                tcs.TrySetResult(dumpStr);
                }
            };
            if (driver.Init())
            {
                DeviceV7 dev;
                if (LBAsked.LBIpString == "LKDSCloud")
                {

                    dev = new DeviceV7()
                    {
                        IP = DeviceV7.CloudIP,
                        Port = DeviceV7.CloudPort,
                        UnitID = (uint)LBAsked.LBId,
                        Pass = LBAsked.LBKey
                    };
                } else
                {
                    dev = new DeviceV7()
                    {
                        IP = LBAsked.LBIpString,
                        Port = LBAsked.LBPort,
                        UnitID = (uint)LBAsked.LBId,
                        Pass = LBAsked.LBKey
                    };
                }

                driver.AddDevice(ref dev);
                for (int i = StartByte; i < DumpSizeButes; i += PacketSize)
                {
                    PackV7NVRAMAsk nvramAsk = new PackV7NVRAMAsk();
                    Union16 union16 = new Union16();
                    union16.Value = (short)i;
                    nvramAsk.Address = union16;
                    nvramAsk.NVRAMLen = (byte)PacketSize;
                    nvramAsk.isWriteNVRAM = false;
                    nvramAsk.UnitID = 51191;
                    if ((nvramAsk.PackID = driver.SendPack(nvramAsk)) == 0)
                    {
                        Console.WriteLine("Не получилось поставить в очередь пакет");

                    }
                }
            }
            return await tcs.Task;

        }

        public void FromLBToSQLite(List<LB> lBs)
        {
            for(int i = 0; i < lBs.Count;)
            {
                LB lb = lBs[i];
                List<string> ActualDump = GetDump(lb).Result;
                List<string> LastDump = new List<string>();

                // ^ вызов функции по получению последнего дампа из бд
                if(DumpsComparation(LastDump, ActualDump))
                {
                    //дамп отличается
                } else
                {
                    //дамп не отличается
                }
            }
        }

        public bool DumpsComparation(List<string> a, List<string> b)
        {
            bool flag = false;

            for(int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i])
                {
                    flag = true;
                }
            }

            return flag;
        }
        public void DBInitiate()
        {

            string baseName = "LBDumps.db3";
            SQLiteConnection.CreateFile(baseName);
            SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
            using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
            {
                connection.ConnectionString = "Data Source = " + baseName;
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"CREATE TABLE IF NOT EXISTS [LBs] (
                    [inner_id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
                    [id] integer NOT NULL,
                    [name] char(100) NOT NULL,
                    [key] char(100) NOT NULL,
                    [ip] char(100) NOT NULL,
                    [port] integer NOT NULL,
                    [status] char(100) NOT NULL,
                    [last_change] char(100) NOT NULL,
                    [las_dump] char(100) NOT NULL
                    );";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"CREATE TABLE IF NOT EXISTS [Dumps] (
                    [idDump] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
                    [id] integer NOT NULL,
                    [data] char(150) NOT NULL,
                    [time] char(100) NOT NULL
                    );";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
