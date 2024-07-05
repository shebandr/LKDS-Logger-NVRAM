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
using System.Windows.Markup;
using System.Runtime.Remoting;
using System.Data.SqlClient;
using System.IO;
using System.Collections.ObjectModel;

namespace LKDS_Logger_NVRAM
{
    internal class LBAddConnect
    {
        public LBAddConnect() { }
        private string baseName = "LBDumps.db3";

        private static ManualResetEvent doneEvent = new ManualResetEvent(false);
        public Dump GetDump(LB LBAsked)
        {
            int DumpSizeBites = 64;
            int PacketSize = 64;
            int BytesCount = 0;
            int StartByte = 0;
            List<byte> dumpBytes = new List<byte>();
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            int flag = 0;
            DriverV7Net driver = new DriverV7Net();
            driver.OnReceiveData = delegate (PackV7 pack)
            {
                Console.WriteLine("делегированная функция запущена");
                if (pack is LKDSFramework.Packs.DataDirect.PackV7NVRAMAns)
                {
                    LKDSFramework.Packs.DataDirect.PackV7NVRAMAns dump = pack as LKDSFramework.Packs.DataDirect.PackV7NVRAMAns;
                    Console.Write("данные получены: " + dump.Data.Count() + " ");
                    foreach (var i in dump.Data)
                    {
                        if (flag > 3)
                        {
                            BytesCount++;
                            dumpBytes.Add(i);
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
                }
                else
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
                    nvramAsk.UnitID = (uint)LBAsked.LBId;
                    if ((nvramAsk.PackID = driver.SendPack(nvramAsk)) == 0)
                    {
                        Console.WriteLine("Не получилось поставить в очередь пакет");

                        Dump dump = new Dump();
                        dump.id = -1;
                        return dump;
                    }
                }
            }
            Thread.Sleep(200);
            while (true)
            {
  /*              doneEvent.WaitOne();*/

                Console.WriteLine("должно начать работать " + BytesCount + " " + DumpSizeBites);
                if (BytesCount - 1 == DumpSizeBites)
                {
                    string tempStrDump = BitConverter.ToString(dumpBytes.ToArray()).Replace("-", string.Empty);
                    Console.WriteLine("длина костылей: " + dumpBytes.Count.ToString() + " " + tempStrDump.Length + " " + tempStrDump);
                    string dumpStr = "";
                    for (int i = 0; i < dumpBytes.Count; i++)
                    {
                        dumpStr += tempStrDump[i * 2].ToString() + tempStrDump[i * 2 + 1].ToString() + " ";
                    }
                    driver.Close();
                    Dump dump = new Dump();
                    dump.TimeDate = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                    dump.Data = dumpStr;
                    Console.WriteLine("дамп возвращен");
                    return dump;
                }
                else
                {

                    Dump dump = new Dump();
                    dump.id = -1;
                    Console.WriteLine("дамп проблемный");

                    return dump;
                    //
                }
            }

        }

        public void GetDumpFromLBToSQL(LB lb, int index, MainWindow MW)
        {
            Console.WriteLine(lb.LBId + " " + index); 
            Dump ActualDump = GetDump(lb);
            Console.WriteLine(lb.LBId + " " + index);
            if (ActualDump.id == -1)
            {
                lb.LBStatus = "не отвечает";
                return;
            }
            else
            {
                lb.LBStatus = "отвечает";
            }
            Dump LastDump = GetLastDump(lb.LBId);
            string currentTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");

            if (DumpsComparation(LastDump, ActualDump))
            {
                DumpToSQL(ActualDump, lb.LBId);
                lb.LBLastChange = currentTime;
                lb.LBLastDump = currentTime;
                MW.LBs[index] = lb;
                MW.LBListForDetached[index] = lb;
                UpdateLastChange(lb.LBId, currentTime);
                UpdateLastDump(lb.LBId, currentTime);

                //дамп отличается
            }
            else
            {
                DumpEdit(lb.LBId);
                MW.LBs[index].LBLastDump = currentTime;

                Console.WriteLine(lb.LBLastDump + " " + MW.LBs[index].LBLastDump);
                MW.LBListForDetached[index] = lb;
                UpdateLastDump(lb.LBId, currentTime);

                //дамп не отличается
            }
        }

        public bool DumpsComparation(Dump a, Dump b)
        {
            bool flag = false;
            if (a.Data == null)
            {
                return true;
            }
            for (int i = 0; i < a.Data.Length; i++)
            {
                if (a.Data[i] != b.Data[i])
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
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"CREATE TABLE IF NOT EXISTS [Settings] (
            [inner_id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
            [universal_key] char(100) NOT NULL,
            [uk_use] bool NOT NULL,
            [fields_clear] bool NOT NULL,
            [window_close] bool NOT NULL,
            [interval] integer NOT NULL,
            [changes_detect] char(100) NOT NULL,
            [dumping_start] char(100) NOT NULL,
            [identific] integer NOT NULL
            );";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
            UpdateSettings("12345678", false, false, true, 3600, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 1);
        }



        public void UpdateSettings(string universalKey, bool ukUse, bool fieldsClear, bool windowClose, int interval, string changesDetect, string dumpingStart, int identific)
        {
            string connectionString = "Data Source=" + baseName + ";Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    // Проверяем, существует ли уже запись
                    command.CommandText = "SELECT COUNT(*) FROM Settings";
                    int count = Convert.ToInt32(command.ExecuteScalar());

                    if (count > 0)
                    {
                        // Обновляем существующую запись
                        command.CommandText = @"UPDATE Settings SET 
                    universal_key = @universalKey, 
                    uk_use = @ukUse, 
                    fields_clear = @fieldsClear, 
                    window_close = @windowClose, 
                    interval = @interval, 
                    changes_detect = @changesDetect, 
                    dumping_start = @dumpingStart, 
                    identific = @identific";
                    }
                    else
                    {
                        // Вставляем новую запись
                        command.CommandText = @"INSERT INTO Settings (
                    universal_key, 
                    uk_use, 
                    fields_clear, 
                    window_close, 
                    interval, 
                    changes_detect, 
                    dumping_start, 
                    identific) 
                    VALUES (
                    @universalKey, 
                    @ukUse, 
                    @fieldsClear, 
                    @windowClose, 
                    @interval, 
                    @changesDetect, 
                    @dumpingStart, 
                    @identific)";
                    }

                    command.Parameters.AddWithValue("@universalKey", universalKey);
                    command.Parameters.AddWithValue("@ukUse", ukUse);
                    command.Parameters.AddWithValue("@fieldsClear", fieldsClear);
                    command.Parameters.AddWithValue("@windowClose", windowClose);
                    command.Parameters.AddWithValue("@interval", interval);
                    command.Parameters.AddWithValue("@changesDetect", changesDetect);
                    command.Parameters.AddWithValue("@dumpingStart", dumpingStart);
                    command.Parameters.AddWithValue("@identific", identific);

                    command.ExecuteNonQuery();
                }
            }
        }


        public List<Dictionary<string, object>> GetAllSettings()
        {
            string connectionString = "Data Source=" + baseName + ";Version=3;";
            List<Dictionary<string, object>> settingsList = new List<Dictionary<string, object>>();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM Settings", connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Dictionary<string, object> settings = new Dictionary<string, object>
                    {
                        { "inner_id", reader["inner_id"] },
                        { "universal_key", reader["universal_key"] },
                        { "uk_use", reader["uk_use"] },
                        { "fields_clear", reader["fields_clear"] },
                        { "window_close", reader["window_close"] },
                        { "interval", reader["interval"] },
                        { "changes_detect", reader["changes_detect"] },
                        { "dumping_start", reader["dumping_start"] },
                        { "identific", reader["identific"] }
                    };
                            settingsList.Add(settings);
                        }
                    }
                }
            }

            return settingsList;
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
                }
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"CREATE TABLE IF NOT EXISTS [dump_" + lb.LBId + "] ( [id] integer PRIMARY KEY AUTOINCREMENT NOT NULL, [data] char(800) NOT NULL, [time] char(100) NOT NULL);";
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
                string dropTableQuery = "DROP TABLE IF EXISTS dump_" + idToDelete.ToString();
                using (SQLiteCommand dropCommand = new SQLiteCommand(dropTableQuery, connection))
                {
                    int rowsAffected = dropCommand.ExecuteNonQuery();
                    Console.WriteLine("удалено: " + rowsAffected);
                }
            }
        }
        /*        // ОБЕ ФУНКЦИИ НИЖЕ ЕЩЕ НЕ ПРОВЕРЕНЫ И МОГУТ НЕ РАБОТАТЬ*/

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

                            dump.id = reader.GetInt32(reader.GetOrdinal("id"));
                            dump.Data = reader.GetString(reader.GetOrdinal("data"));
                            dump.TimeDate = reader.GetString(reader.GetOrdinal("time"));
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

                string selectQuery = $"SELECT * FROM {tableName} ORDER BY id DESC LIMIT 1";

                using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dump.id = reader.GetInt32(reader.GetOrdinal("id"));
                            dump.Data = reader.GetString(reader.GetOrdinal("data"));
                            dump.TimeDate = reader.GetString(reader.GetOrdinal("time"));
                        }
                        else
                        {
                            Console.WriteLine("No records found. " + string.IsNullOrEmpty(dump.Data));
                        }
                    }
                }
            }




            return dump;
        }

        public void DumpToSQL(Dump dump, int LBId)
        {
            string connectionString = "Data Source=" + baseName + ";Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = $"INSERT INTO dump_{LBId} (data, time) VALUES (@data, @time)";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {

                    command.Parameters.AddWithValue("@data", dump.Data);
                    command.Parameters.AddWithValue("@time", dump.TimeDate);

                    int rowsAffected = command.ExecuteNonQuery();
                    Console.WriteLine($"Rows affected Dump to SQL: {rowsAffected}");
                }
            }
        }


        public void DumpEdit(int LBId)
        {
            string connectionString = "Data Source=" + baseName + ";Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string selectQuery = $"SELECT id FROM dump_{LBId} ORDER BY id DESC LIMIT 1";
                int lastIdDump = -1;

                using (SQLiteCommand selectCommand = new SQLiteCommand(selectQuery, connection))
                {
                    object result = selectCommand.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        lastIdDump = Convert.ToInt32(result);
                    }
                }

                if (lastIdDump != -1)
                {
                    // Обновляем поле time в последней записи
                    string newTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string updateQuery = $"UPDATE dump_{LBId} SET time = @newTime WHERE id = @lastIdDump";

                    using (SQLiteCommand updateCommand = new SQLiteCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@newTime", newTime);
                        updateCommand.Parameters.AddWithValue("@lastIdDump", lastIdDump);

                        int rowsAffected = updateCommand.ExecuteNonQuery();
                        Console.WriteLine($"Rows affected: {rowsAffected}");
                    }
                }
                else
                {
                    Console.WriteLine("No records found.");
                }
            }
        }


        public void UpdateLastChange(int id, string newLastChange)
        {
            string connectionString = $"Data Source={baseName};Version=3;";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = "UPDATE LBs SET last_change = @newLastChange WHERE id = @id";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@newLastChange", newLastChange);
                    command.Parameters.AddWithValue("@id", id);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Last Change updated successfully.");
                    }
                    else
                    {
                        Console.WriteLine("No Last Change record found with the specified id.");
                    }
                }
            }

        }
        public void UpdateLastDump(int id, string newLastDump)
        {
            string connectionString = $"Data Source={baseName};Version=3;";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = "UPDATE LBs SET last_dump = @newLastChange WHERE id = @id";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@newLastChange", newLastDump);
                    command.Parameters.AddWithValue("@id", id);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Last Dump Record updated successfully.");
                    }
                    else
                    {
                        Console.WriteLine("No Last Dump  record found with the specified id.");
                    }
                }
            }

        }


    }
 }
