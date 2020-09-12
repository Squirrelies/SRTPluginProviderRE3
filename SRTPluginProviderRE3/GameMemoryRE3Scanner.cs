using ProcessMemory.x64;
using static ProcessMemory.Common.Extensions;
using SRTPluginProviderRE3.Structures;
using System;
using System.Diagnostics;

namespace SRTPluginProviderRE3
{
    internal class GameMemoryRE3Scanner : IDisposable
    {
        // Variables
        private ProcessMemoryHandler memoryAccess;
        private GameMemoryRE3 gameMemoryValues;
        public bool HasScanned;
        public bool ProcessRunning => memoryAccess != null && memoryAccess.ProcessRunning;
        public int ProcessExitCode => (memoryAccess != null) ? memoryAccess.ProcessExitCode : 0;
        private int EnemyTableCount;

        // Pointer Address Variables
        private int pointerAddressIGT;
        private int pointerAddressRank;
        private int pointerAddressSaves;
        private int pointerAddressMapID;
        private int pointerAddressFrameDelta;
        private int pointerAddressState;
        private int pointerAddressHP;
        private int pointerAddressInventory;
        private int pointerAddressEnemy;
        private int pointerAddressDeathCount;
        private int pointerAddressDifficulty;

        // Pointer Classes
        private IntPtr BaseAddress { get; set; }
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
            memoryAccess = new ProcessMemoryHandler(pid);
            if (ProcessRunning)
            {
                BaseAddress = ProcessMemory.Common.NativeWrappers.GetProcessBaseAddress(pid, ProcessMemory.Common.PInvoke.ListModules.LIST_MODULES_64BIT); // Bypass .NET's managed solution for getting this and attempt to get this info ourselves via PInvoke since some users are getting 299 PARTIAL COPY when they seemingly shouldn't.

                // Setup the pointers.
                PointerIGT = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressIGT), 0x60);
                PointerRank = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressRank));
                PointerSaves = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressSaves), 0x198);
                PointerMapID = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressMapID));
                PointerFrameDelta = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressFrameDelta));
                PointerState = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressState));
                PointerPlayerHP = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressHP), 0x50, 0x20);

                PointerEnemyEntryCount = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressEnemy), 0x30);
                GenerateEnemyEntries();

                PointerInventoryCount = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressInventory), 0x50);
                PointerInventoryEntries = new MultilevelPointer[20];
                for (int i = 0; i < PointerInventoryEntries.Length; ++i)
                    PointerInventoryEntries[i] = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressInventory), 0x50, 0x98, 0x10, 0x20 + (i * 0x08), 0x18);

                PointerDeathCount = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressDeathCount));
                PointerDifficulty = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressDifficulty), 0x20, 0x50);
            }
        }

        private bool SelectPointerAddresses(GameVersion version)
        {
            switch (version)
            {
                case GameVersion.RE3_WW_20200806_1:
                    {
                        pointerAddressFrameDelta = 0x08CEA790;
                        pointerAddressMapID = 0x054E30F8;
                        pointerAddressSaves = 0x08DB3BB0;
                        pointerAddressDeathCount = 0x08DB3BB0;
                        pointerAddressDifficulty = 0x08D88C60;
                        pointerAddressState = 0x08DBCD10;
                        pointerAddressIGT = 0x08DB7B90;
                        pointerAddressRank = 0x08D85680;
                        pointerAddressHP = 0x08D89B90;
                        pointerAddressInventory = 0x08D89B90;
                        pointerAddressEnemy = 0x08D8A8A0;

                        return true;
                    }

                case GameVersion.BIO3_CEROZ_20200806_1:
                    {
                        pointerAddressFrameDelta = 0x08CEA790;
                        pointerAddressMapID = 0x054E30F8;
                        pointerAddressSaves = 0x08DB3BB0;
                        pointerAddressDeathCount = 0x08DB3BB0;
                        pointerAddressDifficulty = 0x08D88C60;
                        pointerAddressState = 0x08DBCD10;
                        pointerAddressIGT = 0x08DB7B90;
                        pointerAddressRank = 0x08D85680;
                        pointerAddressHP = 0x08D89B90;
                        pointerAddressInventory = 0x08D89B90;
                        pointerAddressEnemy = 0x08D8A8A0;

                        return true;
                    }

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
        private unsafe void GenerateEnemyEntries()
        {
            fixed (int* p = &EnemyTableCount)
                PointerEnemyEntryCount.TryDerefInt(0x1C, p); // Get the size of the enemy pointer table. This seems to double (4, 8, 16, 32, ...) but never decreases, even after a new game is started.
            if (PointerEnemyEntries == null || PointerEnemyEntries.Length != EnemyTableCount) // Enter if the pointer table is null (first run) or the size does not match.
            {
                PointerEnemyEntries = new MultilevelPointer[EnemyTableCount]; // Create a new enemy pointer table array with the detected size.
                for (int i = 0; i < PointerEnemyEntries.Length; ++i) // Loop through and create all of the pointers for the table.
                    PointerEnemyEntries[i] = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressEnemy), 0x30, 0x20 + (i * 0x08), 0x300);
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

        internal unsafe IGameMemoryRE3 Refresh()
        {
            // Frame Delta
            fixed (float* p = &gameMemoryValues._frameDelta)
                PointerFrameDelta.TryDerefFloat(0x388, p);

            // State
            fixed (byte* p = &gameMemoryValues._isRunning)
                PointerState.TryDerefByte(0x130, p);
            fixed (byte* p = &gameMemoryValues._isCutscene)
                PointerState.TryDerefByte(0x131, p);
            fixed (byte* p = &gameMemoryValues._isMenu)
                PointerState.TryDerefByte(0x132, p);
            fixed (byte* p = &gameMemoryValues._isPaused)
                PointerState.TryDerefByte(0x133, p);

            // IGT
            fixed (long* p = &gameMemoryValues._igtRunningTimer)
                PointerIGT.TryDerefLong(0x18, p);
            fixed (long* p = &gameMemoryValues._igtCutsceneTimer)
                PointerIGT.TryDerefLong(0x20, p);
            fixed (long* p = &gameMemoryValues._igtMenuTimer)
                PointerIGT.TryDerefLong(0x28, p);
            fixed (long* p = &gameMemoryValues._igtPausedTimer)
                PointerIGT.TryDerefLong(0x30, p);

            // Player HP
            fixed (int* p = &gameMemoryValues._playerMaxHealth)
                PointerPlayerHP.TryDerefInt(0x54, p);
            fixed (int* p = &gameMemoryValues._playerCurrentHealth)
                PointerPlayerHP.TryDerefInt(0x58, p);
            fixed (int* p = &gameMemoryValues._rank)
                PointerRank.TryDerefInt(0x58, p);
            fixed (float* p = &gameMemoryValues._rankScore)
                PointerRank.TryDerefFloat(0x5C, p);
            fixed (int* p = &gameMemoryValues._saves)
                PointerSaves.TryDerefInt(0x24, p);
            fixed (int* p = &gameMemoryValues._mapID)
                PointerMapID.TryDerefInt(0x88, p);

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
                    fixed (int* p = &gameMemoryValues.EnemyHealth[i]._maximumHP)
                        PointerEnemyEntries[i].TryDerefInt(0x54, p);
                    fixed (int* p = &gameMemoryValues.EnemyHealth[i]._currentHP)
                        PointerEnemyEntries[i].TryDerefInt(0x58, p);
                }
                else
                { // We're beyond the current size of the enemy table. It must have shrunk because it was larger before but for the sake of performance, we're not going to constantly recreate the array any time the size doesn't match. Just blank out the remaining array values.
                    gameMemoryValues.EnemyHealth[i].MaximumHP = 0;
                    gameMemoryValues.EnemyHealth[i].CurrentHP = 0;
                }
            }

            // Inventory
            fixed (int* p = &gameMemoryValues._playerInventoryCount)
                PointerInventoryCount.TryDerefInt(0x90, p);
            if (gameMemoryValues.PlayerInventory == null)
            {
                gameMemoryValues.PlayerInventory = new InventoryEntry[20];
                for (int i = 0; i < gameMemoryValues.PlayerInventory.Length; ++i)
                {
                    gameMemoryValues.PlayerInventory[i] = new InventoryEntry() { _slotPosition = -1, _data = new int[5] };
                    Array.Copy(InventoryEntry.EMPTY_INVENTORY_ITEM, 0, gameMemoryValues.PlayerInventory[i]._data, 0, 5);
                }
            }
            for (int i = 0; i < PointerInventoryEntries.Length; ++i)
            {
                if (i < gameMemoryValues.PlayerInventoryCount)
                {
                    try
                    {
                        fixed (long* p = &gameMemoryValues.PlayerInventory[i]._invDataOffset)
                            PointerInventoryEntries[i].TryDerefLong(0x10, p);
                        gameMemoryValues.PlayerInventory[i]._invDataOffset -= PointerInventoryEntries[i].Address.ToInt64();

                        fixed (int* p = &gameMemoryValues.PlayerInventory[i]._slotPosition)
                            PointerInventoryEntries[i].TryDerefInt(0x28, p);
                        fixed (int* p = &gameMemoryValues.PlayerInventory[i]._data[0])
                            PointerInventoryEntries[i].TryDerefByteArray((int)gameMemoryValues.PlayerInventory[i]._invDataOffset + 0x10, 20, (byte*)p);
                    }
                    catch
                    {
                        fixed (int* p = &gameMemoryValues.PlayerInventory[i]._slotPosition)
                            PointerInventoryEntries[i].TryDerefInt(0x28, p);
                        gameMemoryValues.PlayerInventory[i]._data = InventoryEntry.EMPTY_INVENTORY_ITEM;
                    }
                }
                else
                {
                    fixed (int* p = &gameMemoryValues.PlayerInventory[i]._slotPosition)
                        PointerInventoryEntries[i].TryDerefInt(0x28, p);
                    gameMemoryValues.PlayerInventory[i]._data = InventoryEntry.EMPTY_INVENTORY_ITEM;
                }
            }

            // Other stats and info.
            fixed (int* p = &gameMemoryValues._playerDeathCount)
                PointerDeathCount.TryDerefInt(0xC0, p);
            fixed (int* p = &gameMemoryValues._difficulty)
                PointerDifficulty.TryDerefInt(0x78, p);

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
