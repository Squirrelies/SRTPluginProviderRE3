using SRTPluginProviderRE3.Structures;
using System;
using System.Globalization;

namespace SRTPluginProviderRE3
{
    public struct GameMemoryRE3 : IGameMemoryRE3
    {
        private const string IGT_TIMESPAN_STRING_FORMAT = @"hh\:mm\:ss\.fff";

        public int PlayerCurrentHealth { get; set; }

        public int PlayerMaxHealth { get; set; }

        public InventoryEntry[] PlayerInventory { get; set; }

        public EnemyHP[] EnemyHealth { get; set; }

        public long IGTRunningTimer { get; set; }

        public long IGTCutsceneTimer { get; set; }

        public long IGTMenuTimer { get; set; }

        public long IGTPausedTimer { get; set; }

        public int Rank { get; set; }

        public float RankScore { get; set; }

        public int Saves { get; set; }

        public int MapID { get; set; }

        public float FrameDelta { get; set; }

        public int State { get; set; }

        // Public Properties - Calculated
        public long IGTCalculated => unchecked(IGTRunningTimer - IGTCutsceneTimer - IGTPausedTimer);

        public long IGTCalculatedTicks => unchecked(IGTCalculated * 10L);

        public TimeSpan IGTTimeSpan
        {
            get
            {
                TimeSpan timespanIGT;

                if (IGTCalculatedTicks <= TimeSpan.MaxValue.Ticks)
                    timespanIGT = new TimeSpan(IGTCalculatedTicks);
                else
                    timespanIGT = new TimeSpan();

                return timespanIGT;
            }
        }

        public string IGTFormattedString => IGTTimeSpan.ToString(IGT_TIMESPAN_STRING_FORMAT, CultureInfo.InvariantCulture);
    }
}
