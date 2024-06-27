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

        public DetachedAsks(Mutex mw, int time, List<LB> LBs)
        {
            RunPeriodicallyAsync(time, mw, LBs);
        }


        public static async Task RunPeriodicallyAsync(int intervalSeconds, Mutex MW, List<LB> LBs)
        {
            if (!MW.WaitOne(TimeSpan.Zero, true))
            {
                Console.WriteLine("Another instance is already running.");
                return;
            }

            try
            {
                while (true)
                {

                    MW.WaitOne();
                    foreach(var lb in LBs)
                    {
                        lBAddConnect.GetDumpFromLBToSQL(lb);

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
