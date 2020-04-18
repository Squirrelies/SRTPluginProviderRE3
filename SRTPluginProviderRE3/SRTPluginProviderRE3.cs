using SRTPluginBase;
using System;
using System.Diagnostics;
using System.Linq;

namespace SRTPluginProviderRE3
{
    public class SRTPluginProviderRE3 : IPluginProvider
    {
        private GameMemoryRE3Scanner gameMemoryScanner;
        private Stopwatch stopwatch;
        private IPluginHostDelegates hostDelegates;
        public IPluginInfo Info => new PluginInfo();

        public int Startup(IPluginHostDelegates hostDelegates)
        {
            this.hostDelegates = hostDelegates;
            gameMemoryScanner = new GameMemoryRE3Scanner(Process.GetProcessesByName("re3").First().Id);
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
    }
}
