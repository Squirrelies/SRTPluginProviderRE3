using SRTPluginBase;
using System;

namespace SRTPluginProviderRE3
{
    internal class PluginInfo : IPluginInfo
    {
        public string Name => "Game Memory Provider (Resident Evil 3 (2020))";

        public string Description => "A game memory provider plugin for Resident Evil 3 (2020).";

        public string Author => "Squirrelies";

        public Uri MoreInfoURL => new Uri("https://github.com/Squirrelies/SRTPluginProviderRE3");

        public int VersionMajor => assemblyFileVersion.ProductMajorPart;

        public int VersionMinor => assemblyFileVersion.ProductMinorPart;

        public int VersionBuild => assemblyFileVersion.ProductBuildPart;

        public int VersionRevision => assemblyFileVersion.ProductPrivatePart;

        private System.Diagnostics.FileVersionInfo assemblyFileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
    }
}
