using SRTPluginProviderRE3.Structures;
using System;

namespace SRTPluginProviderRE3
{
    public interface IGameMemoryRE3
    {
        int PlayerCurrentHealth { get; set; }
        int PlayerMaxHealth { get; set; }
        int PlayerDeathCount { get; set; }
        int PlayerInventoryCount { get; set; }
        InventoryEntry[] PlayerInventory { get; set; }
        EnemyHP[] EnemyHealth { get; set; }
        long IGTRunningTimer { get; set; }
        long IGTCutsceneTimer { get; set; }
        long IGTMenuTimer { get; set; }
        long IGTPausedTimer { get; set; }
        int Difficulty { get; set; }
        int Rank { get; set; }
        float RankScore { get; set; }
        int Saves { get; set; }
        int MapID { get; set; }
        float FrameDelta { get; set; }
        int State { get; set; }
        long IGTCalculated { get; }
        long IGTCalculatedTicks { get; }
        TimeSpan IGTTimeSpan { get; }
        string IGTFormattedString { get; }
        string DifficultyName { get; }
        string ScoreName { get; }
    }
}
