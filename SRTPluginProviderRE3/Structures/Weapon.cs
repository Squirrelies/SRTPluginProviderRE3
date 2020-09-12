using System;
using System.Runtime.InteropServices;

namespace SRTPluginProviderRE3.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Weapon : IEquatable<Weapon>
    {
        public WeaponEnumeration WeaponID;
        public AttachmentsFlag Attachments;

        public bool Equals(Weapon other) => (int)WeaponID == (int)other.WeaponID && (int)Attachments == (int)other.Attachments;
    }
}
