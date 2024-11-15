﻿using LKDSFramework.Packs;
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
using System.Windows.Markup;
using System.Runtime.Remoting;

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
        private string baseName = "LBDumps.db3";

        private static ManualResetEvent doneEvent = new ManualResetEvent(false);
        public async Task<List<string>> GetDump(LB LBAsked)
        {
            int DumpSizeBites = 64;
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
                            BytesCount++;
                        }
                        flag++;
                        Console.Write(i + " ");
                        

                    }
                    Console.WriteLine();



                }

                Console.Write("костыль в асинке: " + dumpBytes.Count());
                if (!doneEvent.SafeWaitHandle.IsClosed)
                {
                    doneEvent.Set();
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
                for (int i = StartByte; i < DumpSizeBites; i += PacketSize)
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

            while (true)
            {
                doneEvent.WaitOne();
                Console.WriteLine("должно начать работать " + BytesCount + " " + DumpSizeBites);
                if (BytesCount == DumpSizeBites)
                {
                    string tempStrDump = BitConverter.ToString(dumpBytes.ToArray()).Replace("-", string.Empty);
                    Console.WriteLine("длина костылей: " + dumpBytes.Count.ToString() + " " + tempStrDump.Length + " " + tempStrDump);
                    List<string> dumpStr = new List<string>();
                    for (int i = 0; i < dumpBytes.Count; i++)
                    {
                        dumpStr.Add(tempStrDump[i * 2].ToString() + tempStrDump[i * 2 + 1].ToString());
                    }
                    driver.Close();
                    return dumpStr;
                }
                else
                {
                    doneEvent.Reset();
                }
            }

           

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
                    [last_dump] char(100) NOT NULL
                    );";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }
        public LB LBFromSQL(int lBId)
        {

            // запрос лб в таблице лб

            string connectionString = "Data Source=" + baseName + ";Version=3;";
            LB lB = new LB();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                int idToQuery = lBId; // Замените на нужный вам id
                string query = "SELECT * FROM LBs WHERE id = @id";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", idToQuery);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine(lBId);
                            Console.WriteLine($"inner_id: {reader["inner_id"]}, id: {reader["id"]}, name: {reader["name"]}, key: {reader["key"]}, ip: {reader["ip"]}, port: {reader["port"]}, status: {reader["status"]}, last_change: {reader["last_change"]}, last_dump: {reader["last_dump"]}");
                            lB.LBId = Int32.Parse(reader["id"].ToString());
                            lB.LBName = reader["name"].ToString();
                            lB.LBKey = reader["key"].ToString();
                            lB.LBIpString = reader["ip"].ToString();
                            lB.LBPort = Int32.Parse(reader["port"].ToString());
                            lB.LBLastChange = reader["last_change"].ToString();
                            lB.LBStatus = reader["status"].ToString();
                            lB.LBLastDump = reader["last_dump"].ToString();
                        }
                    }
                }
            }


            return lB;    
        }

        public void LBToSQL(LB lb)
        {
            string connectionString = "Data Source=" + baseName + ";Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string insertQuery = @"INSERT INTO LBs (id, name, key, ip, port, status, last_change, last_dump) 
                                   VALUES (@id, @name, @key, @ip, @port, @status, @last_change, @last_dump)";

                using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@id", lb.LBId); 
                    command.Parameters.AddWithValue("@name", lb.LBName);
                    command.Parameters.AddWithValue("@key", lb.LBKey);
                    command.Parameters.AddWithValue("@ip", lb.LBIpString);
                    command.Parameters.AddWithValue("@port", lb.LBPort);
                    command.Parameters.AddWithValue("@status", lb.LBStatus);
                    command.Parameters.AddWithValue("@last_change", lb.LBLastChange);
                    command.Parameters.AddWithValue("@last_dump", lb.LBLastDump);

                    int rowsAffected = command.ExecuteNonQuery();
                    Console.WriteLine($"Rows affected: {rowsAffected}");
                }
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"CREATE TABLE IF NOT EXISTS [dump_" + lb.LBId + "] ( [idDump] integer PRIMARY KEY AUTOINCREMENT NOT NULL, [data] char(150) NOT NULL, [time] char(100) NOT NULL);";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }
        public List<LB> AllLBIdFromSQL()
        {
            List<LB> LBs = new List<LB>();
            List<int> LBList = new List<int>();
            string connectionString = "Data Source=" + baseName + ";Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT id FROM LBs";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"id: {reader["id"]}");
                            LBs.Add(LBFromSQL(Int32.Parse(reader["id"].ToString())));
                        }
                    }
                }
            }
            return LBs;
        }

        public void LBEditSQL(LB lB)
        {
            string connectionString = "Data Source=your_database.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                int idToUpdate = lB.LBId; 
                string updateQuery = @"UPDATE LBs 
                                   SET name = @name, 
                                       key = @key, 
                                       ip = @ip, 
                                       port = @port, 
                                       status = @status, 
                                       last_change = @last_change, 
                                       last_dump = @last_dump 
                                   WHERE id = @id";

                using (SQLiteCommand command = new SQLiteCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@id", idToUpdate);
                    command.Parameters.AddWithValue("@name", lB.LBName);
                    command.Parameters.AddWithValue("@key", lB.LBKey);
                    command.Parameters.AddWithValue("@ip", lB.LBIpString);
                    command.Parameters.AddWithValue("@port", lB.LBPort);
                    command.Parameters.AddWithValue("@status", lB.LBStatus);
                    command.Parameters.AddWithValue("@last_change", lB.LBLastChange);
                    command.Parameters.AddWithValue("@last_dump", lB.LBLastDump);

                    int rowsAffected = command.ExecuteNonQuery();
                    Console.WriteLine($"Rows affected: {rowsAffected}");
                }
            }
        }
        public void LBDelSQL(LB lB)
        {
            string connectionString = "Data Source=" + baseName + ";Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                int idToDelete = lB.LBId; // Замените на нужный вам id

                // Удаление записи по id
                string deleteRecordQuery = "DELETE FROM LBs WHERE id = @id";
                using (SQLiteCommand deleteCommand = new SQLiteCommand(deleteRecordQuery, connection))
                {
                    deleteCommand.Parameters.AddWithValue("@id", idToDelete);
                    int rowsAffected = deleteCommand.ExecuteNonQuery();
                    Console.WriteLine($"Rows affected by delete: {rowsAffected}");
                }

                // Удаление таблицы с названием, соответствующим этому id
                string dropTableQuery = "DROP TABLE IF EXISTS dump_"+ idToDelete.ToString();
                using (SQLiteCommand dropCommand = new SQLiteCommand(dropTableQuery, connection))
                {
                    int rowsAffected = dropCommand.ExecuteNonQuery();
                    Console.WriteLine("удалено: " + rowsAffected);
                }
            }
        }
        // ОБЕ ФУНКЦИИ НИЖЕ ЕЩЕ НЕ ПРОВЕРЕНЫ И МОГУТ НЕ РАБОТАТЬ

        public List<Dump> GetAllDumps(int lBid)
        {
            List<Dump> dumps = new List<Dump>();
            string connectionString = "Data Source=" + baseName + ";Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                
                string tableName = $"dump_{lBid}";

                string selectQuery = $"SELECT * FROM {tableName}";

                using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Dump dump = new Dump();

                            dump.id = reader.GetInt32(reader.GetOrdinal("idDump"));
                            dump.Data = reader.GetString(reader.GetOrdinal("data"));
                            dump.TimeDate = reader.GetString(reader.GetOrdinal("time"));

                            Console.WriteLine($"id: {dump.id}, data: {dump.Data}, time: {dump.TimeDate}");
                            dumps.Add(dump);
                        }
                    }
                }
            }


            return dumps;
        }
        public Dump GetLastDump(int lBId) 
        {
            Dump dump = new Dump();
            string connectionString = "Data Source=" + baseName + ";Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

 
                string tableName = $"dump_{lBId}";

                string selectQuery = $"SELECT * FROM {tableName} ORDER BY idDump DESC LIMIT 1";

                using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dump.id = reader.GetInt32(reader.GetOrdinal("idDump"));
                            dump.Data = reader.GetString(reader.GetOrdinal("data"));
                            dump.TimeDate = reader.GetString(reader.GetOrdinal("time"));
                            Console.WriteLine($"id: {dump.id}, data: {dump.Data}, time: {dump.TimeDate}");
                        }
                        else
                        {
                            Console.WriteLine("No records found.");
                        }
                    }
                }
            }




            return dump;
        }




    }
}
