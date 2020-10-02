using SRTPluginProviderRE3.Structures;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SRTPluginProviderRE3
{
    public class GameMemoryRE3 : IGameMemoryRE3
    {
        private const string IGT_TIMESPAN_STRING_FORMAT = @"hh\:mm\:ss\.fff";

        public int PlayerCurrentHealth { get => _playerCurrentHealth; set => _playerCurrentHealth = value; }
        internal int _playerCurrentHealth;

        public int PlayerMaxHealth { get => _playerMaxHealth; set => _playerMaxHealth = value; }
        internal int _playerMaxHealth;

        public int PlayerDeathCount { get => _playerDeathCount; set => _playerDeathCount = value; }
        internal int _playerDeathCount;

        public int PlayerInventoryCount { get => _playerInventoryCount; set => _playerInventoryCount = value; }
        internal int _playerInventoryCount;

        public InventoryEntry[] PlayerInventory { get => _playerInventory; set => _playerInventory = value; }
        internal InventoryEntry[] _playerInventory;

        public EnemyHP[] EnemyHealth { get => _enemyHealth; set => _enemyHealth = value; }
        internal EnemyHP[] _enemyHealth;

        public long IGTRunningTimer { get => _igtRunningTimer; set => _igtRunningTimer = value; }
        internal long _igtRunningTimer;

        public long IGTCutsceneTimer { get => _igtCutsceneTimer; set => _igtCutsceneTimer = value; }
        internal long _igtCutsceneTimer;

        public long IGTMenuTimer { get => _igtMenuTimer; set => _igtMenuTimer = value; }
        internal long _igtMenuTimer;

        public long IGTPausedTimer { get => _igtPausedTimer; set => _igtPausedTimer = value; }
        internal long _igtPausedTimer;

        public int Difficulty { get => _difficulty; set => _difficulty = value; }
        internal int _difficulty;

        public int Rank { get => _rank; set => _rank = value; }
        internal int _rank;

        public float RankScore { get => _rankScore; set => _rankScore = value; }
        internal float _rankScore;

        public int Saves { get => _saves; set => _saves = value; }
        internal int _saves;

        public int MapID { get => _mapID; set => _mapID = value; }
        internal int _mapID;

        public float FrameDelta { get => _frameDelta; set => _frameDelta = value; }
        internal float _frameDelta;

        public bool IsRunning { get => _isRunning != 0x00; set => _isRunning = (byte)(value ? 0x01 : 0x00); }
        internal byte _isRunning;

        public bool IsCutscene { get => _isCutscene != 0x00; set => _isCutscene = (byte)(value ? 0x01 : 0x00); }
        internal byte _isCutscene;

        public bool IsMenu { get => _isMenu != 0x00; set => _isMenu = (byte)(value ? 0x01 : 0x00); }
        internal byte _isMenu;

        public bool IsPaused { get => _isPaused != 0x00; set => _isPaused = (byte)(value ? 0x01 : 0x00); }
        internal byte _isPaused;

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
                else if (Difficulty == 1 || Difficulty == 3 || Difficulty == 4)
                {
                    SRank = new TimeSpan(0, 2, 0, 0);
                    BRank = new TimeSpan(0, 4, 0, 0);
                }
                else if (Difficulty == 2)
                {
                    SRank = new TimeSpan(0, 1, 45, 0);
                    BRank = new TimeSpan(0, 4, 0, 0);
                }
                else
                {
                    SRank = new TimeSpan();
                    BRank = new TimeSpan();
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
