﻿using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SRTPluginProviderRE3
{
    /// <summary>
    /// SHA256 hashes for the RE3/BIO3 REmake game executables.
    /// 
    /// Resident Evil 3 (WW): https://steamdb.info/app/952060/ / https://steamdb.info/depot/952062/
    /// Biohazard 3 (CERO Z): https://steamdb.info/app/1100830/ / https://steamdb.info/depot/1100831/
    /// </summary>
    public static class GameHashes
    {
        private static readonly byte[] re3WW_20200603_1 = new byte[32] { 0xA5, 0x27, 0x5D, 0xED, 0xD6, 0xE0, 0x3D, 0xEC, 0x36, 0x1E, 0xA1, 0xFD, 0x08, 0xBC, 0x8E, 0xA6, 0x92, 0x97, 0x4B, 0xB0, 0xD1, 0x50, 0x26, 0x19, 0xF1, 0xC3, 0xD7, 0xB3, 0x08, 0xA4, 0x57, 0x99 };
        private static readonly byte[] bio3CEROZ_20200603_1 = new byte[32] { 0x5D, 0xD3, 0x5A, 0x70, 0x24, 0x10, 0xFF, 0x2F, 0x45, 0xFA, 0xA4, 0x9D, 0x56, 0x0E, 0xB5, 0x48, 0x64, 0x87, 0xB8, 0xBC, 0x84, 0xC7, 0xAC, 0x02, 0xD2, 0xA6, 0x0A, 0x69, 0x2D, 0xA2, 0x2B, 0xCE };

        public static GameVersion DetectVersion(string filePath)
        {
            byte[] checksum;
            using (SHA256 hashFunc = SHA256.Create())
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                checksum = hashFunc.ComputeHash(fs);

            if (checksum.SequenceEqual(re3WW_20200603_1))
                return GameVersion.RE3_WW_20200603_1;
            else if (checksum.SequenceEqual(bio3CEROZ_20200603_1))
                return GameVersion.BIO3_CEROZ_20200603_1;
            else
                return GameVersion.Unknown;
        }
    }
}
