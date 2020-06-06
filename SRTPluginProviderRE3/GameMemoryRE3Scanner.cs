using ProcessMemory;
using SRTPluginProviderRE3.Structures;
using System;
using System.Diagnostics;

namespace SRTPluginProviderRE3
{
    internal class GameMemoryRE3Scanner : IDisposable
    {
        // Variables
        private ProcessMemory.ProcessMemory memoryAccess;
        private GameMemoryRE3 gameMemoryValues;
        public bool HasScanned;
        public bool ProcessRunning => memoryAccess != null && memoryAccess.ProcessRunning;
        public int ProcessExitCode => (memoryAccess != null) ? memoryAccess.ProcessExitCode : 0;
        private int EnemyTableCount;

        // Pointer Address Variables
        private long pointerAddressIGT;
        private long pointerAddressRank;
        private long pointerAddressSaves;
        private long pointerAddressMapID;
        private long pointerAddressFrameDelta;
        private long pointerAddressState;
        private long pointerAddressHP;
        private long pointerAddressInventory;
        private long pointerAddressEnemy;
        private long pointerAddressDeathCount;
        private long pointerAddressDifficulty;

        // Pointer Classes
        private long BaseAddress { get; set; }
        private MultilevelPointer PointerIGT { get; set; }
        private MultilevelPointer PointerRank { get; set; }
        private MultilevelPointer PointerSaves { get; set; }
        private MultilevelPointer PointerMapID { get; set; }
        private MultilevelPointer PointerFrameDelta { get; set; }
        private MultilevelPointer PointerState { get; set; }
        private MultilevelPointer PointerPlayerHP { get; set; }
        private MultilevelPointer PointerEnemyEntryCount { get; set; }
        private MultilevelPointer[] PointerEnemyEntries { get; set; }
        private MultilevelPointer[] PointerInventoryEntries { get; set; }
        private MultilevelPointer PointerInventoryCount { get; set; }
        private MultilevelPointer PointerDeathCount { get; set; }
        private MultilevelPointer PointerDifficulty { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proc"></param>
        internal GameMemoryRE3Scanner(Process process = null)
        {
            gameMemoryValues = new GameMemoryRE3();
            if (process != null)
                Initialize(process);
        }

        internal void Initialize(Process process)
        {
            if (process == null)
                return; // Do not continue if this is null.

            if (!SelectPointerAddresses(GameHashes.DetectVersion(process.MainModule.FileName)))
                return; // Unknown version.

            int pid = GetProcessId(process).Value;
            memoryAccess = new ProcessMemory.ProcessMemory(pid);
            if (ProcessRunning)
            {
                BaseAddress = NativeWrappers.GetProcessBaseAddress(pid, PInvoke.ListModules.LIST_MODULES_64BIT).ToInt64(); // Bypass .NET's managed solution for getting this and attempt to get this info ourselves via PInvoke since some users are getting 299 PARTIAL COPY when they seemingly shouldn't.

                // Setup the pointers.
                PointerIGT = new MultilevelPointer(memoryAccess, BaseAddress + pointerAddressIGT, 0x60L);
                PointerRank = new MultilevelPointer(memoryAccess, BaseAddress + pointerAddressRank);
                PointerSaves = new MultilevelPointer(memoryAccess, BaseAddress + pointerAddressSaves, 0x198L);
                PointerMapID = new MultilevelPointer(memoryAccess, BaseAddress + pointerAddressMapID);
                PointerFrameDelta = new MultilevelPointer(memoryAccess, BaseAddress + pointerAddressFrameDelta);
                PointerState = new MultilevelPointer(memoryAccess, BaseAddress + pointerAddressState);
                PointerPlayerHP = new MultilevelPointer(memoryAccess, BaseAddress + pointerAddressHP, 0x50L, 0x20L);

                PointerEnemyEntryCount = new MultilevelPointer(memoryAccess, BaseAddress + pointerAddressEnemy, 0x30L);
                GenerateEnemyEntries();

                PointerInventoryCount = new MultilevelPointer(memoryAccess, BaseAddress + pointerAddressInventory, 0x50L);
                PointerInventoryEntries = new MultilevelPointer[20];
                for (long i = 0; i < PointerInventoryEntries.Length; ++i)
                    PointerInventoryEntries[i] = new MultilevelPointer(memoryAccess, BaseAddress + pointerAddressInventory, 0x50L, 0x98L, 0x10L, 0x20L + (i * 0x08L), 0x18L);

                PointerDeathCount = new MultilevelPointer(memoryAccess, BaseAddress + pointerAddressDeathCount);
                PointerDifficulty = new MultilevelPointer(memoryAccess, BaseAddress + pointerAddressDifficulty, 0x20L, 0x50L);
            }
        }

        private bool SelectPointerAddresses(GameVersion version)
        {
            switch (version)
            {
                case GameVersion.RE3_WW_20200603_1:
                    {
                        pointerAddressFrameDelta = 0x08C1B4D0;
                        pointerAddressMapID = 0x054190F8;
                        pointerAddressSaves = 0x08CE4720;
                        pointerAddressDeathCount = 0x08CE4720;
                        pointerAddressDifficulty = 0x08CB9598;
                        pointerAddressState = 0x08CEDA98;
                        pointerAddressIGT = 0x08CE8430;
                        pointerAddressRank = 0x08CB62A8;
                        pointerAddressHP = 0x08CBA618;
                        pointerAddressInventory = 0x08CBA618;
                        pointerAddressEnemy = 0x08CB8618;

                        return true;
                    }

                case GameVersion.BIO3_CEROZ_20200603_1:
                    {
                        pointerAddressFrameDelta = 0x08CDD490;
                        pointerAddressMapID = 0x054DB0F8;
                        pointerAddressSaves = 0x08DA66F0;
                        pointerAddressDeathCount = 0x08DA66F0;
                        pointerAddressDifficulty = 0x08D7B548;
                        pointerAddressState = 0x08DAFA70;
                        pointerAddressIGT = 0x08DAA3F0;
                        pointerAddressRank = 0x08D78258;
                        pointerAddressHP = 0x08D7C5E8;
                        pointerAddressInventory = 0x08D7C5E8;
                        pointerAddressEnemy = 0x08D7A5A8;

                        return true;
                    }
            }

            // If we made it this far... rest in pepperonis. We have failed to detect any of the correct versions we support and have no idea what pointer addresses to use. Bail out.
            return false;
        }

        /// <summary>
        /// Dereferences a 4-byte signed integer via the PointerEnemyEntryCount pointer to detect how large the enemy pointer table is and then create the pointer table entries if required.
        /// </summary>
        private void GenerateEnemyEntries()
        {
            EnemyTableCount = PointerEnemyEntryCount.DerefInt(0x1CL); // Get the size of the enemy pointer table. This seems to double (4, 8, 16, 32, ...) but never decreases, even after a new game is started.
            if (PointerEnemyEntries == null || PointerEnemyEntries.Length != EnemyTableCount) // Enter if the pointer table is null (first run) or the size does not match.
            {
                PointerEnemyEntries = new MultilevelPointer[EnemyTableCount]; // Create a new enemy pointer table array with the detected size.
                for (long i = 0; i < PointerEnemyEntries.Length; ++i) // Loop through and create all of the pointers for the table.
                    PointerEnemyEntries[i] = new MultilevelPointer(memoryAccess, BaseAddress + pointerAddressEnemy, 0x30L, 0x20L + (i * 0x08L), 0x300L);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal void UpdatePointers()
        {
            PointerIGT.UpdatePointers();
            PointerPlayerHP.UpdatePointers();
            PointerRank.UpdatePointers();
            PointerSaves.UpdatePointers();
            PointerMapID.UpdatePointers();
            PointerFrameDelta.UpdatePointers();
            PointerState.UpdatePointers();

            PointerEnemyEntryCount.UpdatePointers();
            GenerateEnemyEntries(); // This has to be here for the next part.
            for (int i = 0; i < PointerEnemyEntries.Length; ++i)
                PointerEnemyEntries[i].UpdatePointers();

            PointerInventoryCount.UpdatePointers();
            for (int i = 0; i < PointerInventoryEntries.Length; ++i)
                PointerInventoryEntries[i].UpdatePointers();

            PointerDeathCount.UpdatePointers();
            PointerDifficulty.UpdatePointers();
        }

        internal IGameMemoryRE3 Refresh()
        {
            // Frame Delta
            gameMemoryValues.FrameDelta = PointerFrameDelta.DerefFloat(0x388);

            // State
            gameMemoryValues.IsRunning = PointerState.DerefByte(0x130) != 0x00;
            gameMemoryValues.IsCutscene = PointerState.DerefByte(0x131) != 0x00;
            gameMemoryValues.IsMenu = PointerState.DerefByte(0x132) != 0x00;
            gameMemoryValues.IsPaused = PointerState.DerefByte(0x133) != 0x00;

            // IGT
            gameMemoryValues.IGTRunningTimer = PointerIGT.DerefLong(0x18);
            gameMemoryValues.IGTCutsceneTimer = PointerIGT.DerefLong(0x20);
            gameMemoryValues.IGTMenuTimer = PointerIGT.DerefLong(0x28);
            gameMemoryValues.IGTPausedTimer = PointerIGT.DerefLong(0x30);

            // Player HP
            gameMemoryValues.PlayerMaxHealth = PointerPlayerHP.DerefInt(0x54);
            gameMemoryValues.PlayerCurrentHealth = PointerPlayerHP.DerefInt(0x58);
            gameMemoryValues.Rank = PointerRank.DerefInt(0x58);
            gameMemoryValues.RankScore = PointerRank.DerefFloat(0x5C);
            gameMemoryValues.Saves = PointerSaves.DerefInt(0x24);
            gameMemoryValues.MapID = PointerMapID.DerefInt(0x88);

            // Enemy HP
            GenerateEnemyEntries();
            if (gameMemoryValues.EnemyHealth == null || gameMemoryValues.EnemyHealth.Length < EnemyTableCount)
            {
                gameMemoryValues.EnemyHealth = new EnemyHP[EnemyTableCount];
                for (int i = 0; i < gameMemoryValues.EnemyHealth.Length; ++i)
                    gameMemoryValues.EnemyHealth[i] = new EnemyHP();
            }
            for (int i = 0; i < gameMemoryValues.EnemyHealth.Length; ++i)
            {
                if (i < PointerEnemyEntries.Length)
                { // While we're within the size of the enemy table, set the values.
                    gameMemoryValues.EnemyHealth[i].MaximumHP = PointerEnemyEntries[i].DerefInt(0x54);
                    gameMemoryValues.EnemyHealth[i].CurrentHP = PointerEnemyEntries[i].DerefInt(0x58);
                }
                else
                { // We're beyond the current size of the enemy table. It must have shrunk because it was larger before but for the sake of performance, we're not going to constantly recreate the array any time the size doesn't match. Just blank out the remaining array values.
                    gameMemoryValues.EnemyHealth[i].MaximumHP = 0;
                    gameMemoryValues.EnemyHealth[i].CurrentHP = 0;
                }
            }

            // Inventory
            gameMemoryValues.PlayerInventoryCount = PointerInventoryCount.DerefInt(0x90);
            if (gameMemoryValues.PlayerInventory == null)
            {
                gameMemoryValues.PlayerInventory = new InventoryEntry[20];
                for (int i = 0; i < gameMemoryValues.PlayerInventory.Length; ++i)
                    gameMemoryValues.PlayerInventory[i] = new InventoryEntry();
            }
            for (int i = 0; i < PointerInventoryEntries.Length; ++i)
            {
                if (i < gameMemoryValues.PlayerInventoryCount)
                {
                    try
                    {
                        long invDataOffset = PointerInventoryEntries[i].DerefLong(0x10) - PointerInventoryEntries[i].Address;
                        gameMemoryValues.PlayerInventory[i].SetValues(PointerInventoryEntries[i].DerefInt(0x28), PointerInventoryEntries[i].DerefByteArray(invDataOffset + 0x10, 0x14));
                    }
                    catch
                    {
                        gameMemoryValues.PlayerInventory[i].SetValues(PointerInventoryEntries[i].DerefInt(0x28), null);
                    }
                }
                else
                    gameMemoryValues.PlayerInventory[i].SetValues(PointerInventoryEntries[i].DerefInt(0x28), null);
            }

            // Other stats and info.
            gameMemoryValues.PlayerDeathCount = PointerDeathCount.DerefInt(0xC0);
            gameMemoryValues.Difficulty = PointerDifficulty.DerefInt(0x78);

            HasScanned = true;
            return gameMemoryValues;
        }

        private int? GetProcessId(Process process) => process?.Id;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (memoryAccess != null)
                        memoryAccess.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~REmake1Memory() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
