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
        public int PlayerDeathCount { get; set; }
        public int PlayerInventoryCount { get; set; }

        public InventoryEntry[] PlayerInventory { get; set; }

        public EnemyHP[] EnemyHealth { get; set; }

        public long IGTRunningTimer { get; set; }

        public long IGTCutsceneTimer { get; set; }

        public long IGTMenuTimer { get; set; }

        public long IGTPausedTimer { get; set; }
        public int Difficulty { get; set; }

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

        public string DifficultyName
        {
            get
            {
                switch (Difficulty)
                {
                    case 0:
                        return "Assisted";
                    case 1:
                        return "Standard";
                    case 2:
                        return "Hardcore";
                    case 3:
                        return "Nightmare";
                    case 4:
                        return "Inferno";
                    default:
                        return "Unknown";
                }
            }
        }

        public string ScoreName
        {
            get
            {
                TimeSpan SRank;
                TimeSpan BRank;
                if (Difficulty == 0)
                {
                    SRank = new TimeSpan(0, 2, 30, 0);
                    BRank = new TimeSpan(0, 4, 0, 0);
                }
                else if (Difficulty == 1)
                {
                    SRank = new TimeSpan(0, 2, 0, 0);
                    BRank = new TimeSpan(0, 4, 0, 0);
                }
                else if (Difficulty == 2)
                {
                    SRank = new TimeSpan(0, 1, 45, 0);
                    BRank = new TimeSpan(0, 4, 0, 0);
                }
                else if (Difficulty == 3)
                {
                    SRank = new TimeSpan(0, 2, 0, 0);
                    BRank = new TimeSpan(0, 4, 0, 0);
                }
                else if (Difficulty == 4)
                {
                    SRank = new TimeSpan(0, 2, 0, 0);
                    BRank = new TimeSpan(0, 4, 0, 0);
                }

                if (IGTTimeSpan <= SRank && Saves <= 5)
                    return "S";
                else if (IGTTimeSpan <= SRank && Saves > 5)
                    return "A";
                else if (IGTTimeSpan > SRank && IGTTimeSpan <= BRank)
                    return "B";
                else if (IGTTimeSpan > BRank)
                    return "C";
                else
                    return string.Empty;
            }
        }
    }
}
