using SRTPluginBase;
using System;
using System.Diagnostics;
using System.Linq;

namespace SRTPluginProviderRE3
{
    public class SRTPluginProviderRE3 : IPluginProvider
    {
        private int? processId;
        private GameMemoryRE3Scanner gameMemoryScanner;
        private Stopwatch stopwatch;
        private IPluginHostDelegates hostDelegates;
        public IPluginInfo Info => new PluginInfo();

        public int Startup(IPluginHostDelegates hostDelegates)
        {
            this.hostDelegates = hostDelegates;
            processId = Process.GetProcessesByName("re3")?.FirstOrDefault()?.Id;
            gameMemoryScanner = new GameMemoryRE3Scanner(processId);
            stopwatch = new Stopwatch();
            stopwatch.Start();
            return 0;
        }

        public int Shutdown()
        {
            gameMemoryScanner?.Dispose();
            gameMemoryScanner = null;
            stopwatch?.Stop();
            stopwatch = null;
            return 0;
        }

        public object PullData()
        {
            try
            {
                if (!gameMemoryScanner.ProcessRunning)
                {
                    //hostDelegates.Exit();

                    processId = GetProcessId();
                    if (processId != null)
                        gameMemoryScanner.Initialize(processId.Value); // Re-initialize and attempt to continue.
                    
                    if (!gameMemoryScanner.ProcessRunning)
                    { // Still not running? Restart the timer and return null until the program is running.
                        stopwatch.Restart();
                        return null;
                    }
                }

                if (stopwatch.ElapsedMilliseconds >= 2000L)
                {
                    gameMemoryScanner.UpdatePointers();
                    stopwatch.Restart();
                }
                return gameMemoryScanner.Refresh();
            }
            catch (Exception ex)
            {
                hostDelegates.OutputMessage("[{0}] {1} {2}", ex.GetType().Name, ex.Message, ex.StackTrace);
                return null;
            }
        }

        private int? GetProcessId() => Process.GetProcessesByName("re3")?.FirstOrDefault()?.Id;
    }
}
