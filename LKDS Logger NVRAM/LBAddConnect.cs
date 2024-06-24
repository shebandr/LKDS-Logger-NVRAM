using LKDSFramework.Packs;
using LKDSFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LKDSFramework.Packs.DataDirect;
using System.Windows.Media;

namespace LKDS_Logger_NVRAM
{
    internal class LBAddConnect
    {
        public LBAddConnect() { }
        public void StartService()
        {
            Console.WriteLine("подключение фреймворка");
            DriverV7Net driver = new DriverV7Net();
            // в теории, 4 строки ниже отвечают за получение пакетов
            driver.OnDevChange = delegate (DeviceV7 dev)
            {
                Console.WriteLine($"Устройство {dev} {(dev.isOnline ? "Вышло на связь" : "Ушло со связи")}");
            };
            /*driver.OnSubChange = delegate (SubDeviceV7 sub)
            {
                Console.WriteLine($"Устройство CAN {sub} в {sub.Parrent} {(sub.isOnline ? "Вышло на связь" : "Ушло со связи")}");
            };*/
            /*            driver.OnReceiveData = delegate (PackV7 pack)
                        {
                            if (pack is LKDSFramework.Packs.DataDirect.PackV7WhoAns)
                            {
                                LKDSFramework.Packs.DataDirect.PackV7WhoAns who = pack as LKDSFramework.Packs.DataDirect.PackV7WhoAns;
                                VirtualDeviceV7 dev = who.Device;
                                Console.WriteLine($"Кто ты от {dev.DevClass.GetNameOfEnum()}{who.UnitID}({who.CanID}):{dev.SoftVer}" + " " + who.Data.ToString());
                            }

                        };
            */
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
                PackV7 pack = new PackV7();
                PackV7NVRAMAsk nvramAsk = new PackV7NVRAMAsk();
                Union16 union16 = new Union16();
                union16.Value = 10;
                nvramAsk.Address = union16;
                nvramAsk.NVRAMLen = 128;
                nvramAsk.isWriteNVRAM = false;
                if ((nvramAsk.PackID = driver.SendPack(nvramAsk)) == 0)
                {
                    Console.WriteLine("Не получилось поставить в очередь пакет");
 
                }
                var a = driver.SendPack(nvramAsk);
                Console.WriteLine(a);
                PackV7NVRAMAns nvramAns = new PackV7NVRAMAns(nvramAsk);
                Console.WriteLine("результат запроса: " + nvramAsk.NVRAM.Count().ToString());
                // код выше не работает, почему - хз, сделал как в сендере, завтра надо будет спросить 

                // driver.Close();
            }
        }

    }
}
