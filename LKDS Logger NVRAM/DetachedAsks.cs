using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LKDS_Logger_NVRAM
{
    internal class DetachedAsks
    {
        private static LBAddConnect lBAddConnect = new LBAddConnect();

        public DetachedAsks(Mutex mw, int time, List<LB> LBs, MainWindow MW)
        {
            RunPeriodicallyAsync(time, mw, LBs, MW);
        }


        public static async Task RunPeriodicallyAsync(int intervalSeconds, Mutex MW, List<LB> LBs, MainWindow mw)
        {
            if (!MW.WaitOne(TimeSpan.Zero, true))
            {
                Console.WriteLine("Another instance is already running.");
                return;
            }

            try
            {
                int index= 0;
                while (true)
                {

                    MW.WaitOne();
                    
                    foreach(var lb in LBs)
                    {
                        lBAddConnect.GetDumpFromLBToSQL(lb, index, mw);
                        index++;

                    }
                    Console.WriteLine("1");
                    MW.ReleaseMutex();
                    await Task.Delay(intervalSeconds * 1000); // Задержка на N секунд

                }
            }
            finally
            {
                
            }
        }
    }
}
