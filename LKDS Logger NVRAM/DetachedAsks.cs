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
        private readonly CancellationToken CT;

        public DetachedAsks(Mutex mw, int timeDelay, int time, List<LB> LBs, MainWindow MW, CancellationToken cancellationToken)
        {
            CT = cancellationToken;
            StartWithDelayAsync(timeDelay, time, mw, LBs, MW, CT);
        }

        private async Task StartWithDelayAsync(int timeDelay, int time, Mutex mw, List<LB> LBs, MainWindow MW, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(timeDelay * 1000, cancellationToken);
                await RunPeriodicallyAsync(time, mw, LBs, MW, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Обработка отмены задачи
                Console.WriteLine("Задача была отменена.");
            }
        }

        public static async Task RunPeriodicallyAsync(int intervalSeconds, Mutex MW, List<LB> LBs, MainWindow mw, CancellationToken cancellationToken)
        {
            if (!MW.WaitOne(TimeSpan.Zero, true))
            {
                Console.WriteLine("запущена иная копия");
                return;
            }

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("запрос лб отправляется");
                    MW.WaitOne();

                    try
                    {
                        for (int i = 0; i<LBs.Count; i++)
                        {
                            Console.WriteLine("обработка лб номер " + i + " " + LBs[i].LBName);
                            lBAddConnect.GetDumpFromLBToSQL(LBs[i], i, mw);
                            Console.WriteLine("конец обработки лб номер " + i);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при обработке списка LBs: {ex.Message}");
                    }
                    finally
                    {
                        MW.ReleaseMutex();
                    }

                    await Task.Delay(intervalSeconds * 1000);
                }
            }
            finally
            {
                MW.ReleaseMutex();
            }
        }
        public static async Task RunOnceAsync(Mutex MW, List<LB> LBs, MainWindow mw, CancellationToken cancellationToken)
        {
            if (!MW.WaitOne(TimeSpan.Zero, true))
            {
                Console.WriteLine("запущена иная копия");
                return;
            }

            try
            {
                Console.WriteLine("запрос лб отправляется");
                MW.WaitOne();

                try
                {
                    for (int i = 0; i < LBs.Count; i++)
                    {
                        Console.WriteLine("обработка лб номер " + i + " " + LBs[i].LBName);
                        lBAddConnect.GetDumpFromLBToSQL(LBs[i], i, mw);
                        Console.WriteLine("конец обработки лб номер " + i);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обработке списка LBs: {ex.Message}");
                }
                finally
                {
                    MW.ReleaseMutex();
                }
            }
            finally
            {
                MW.ReleaseMutex();
            }
        }
    }
}
