using SRTPluginBase;
using System;

namespace SRTPluginProviderRE3
{
    internal class PluginInfo : IPluginInfo
    {
        public string Name => "Game Memory Provider (Resident Evil 3 (2020))";

        public string Description => "A game memory provider plugin for Resident Evil 3 (2020).";

        public string Author => "Squirrelies";

        public Uri MoreInfoURL => new Uri("https://github.com/Squirrelies");

        public int VersionMajor => assemblyVersion.Major;

        public int VersionMinor => assemblyVersion.Minor;

        public int VersionBuild => assemblyVersion.Build;

        public int VersionRevision => assemblyVersion.Revision;

        private Version assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
    }
}
