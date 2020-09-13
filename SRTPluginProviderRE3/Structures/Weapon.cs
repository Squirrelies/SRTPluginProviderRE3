using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SRTPluginProviderRE3.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Weapon : IEquatable<Weapon>, IEqualityComparer<Weapon>
    {
        public WeaponEnumeration WeaponID;
        public AttachmentsFlag Attachments;

        public bool Equals(Weapon other) => Equals(this, other);

        public bool Equals(Weapon x, Weapon y) => (int)x.WeaponID == (int)y.WeaponID && (int)x.Attachments == (int)y.Attachments;

        public int GetHashCode(Weapon obj) => (int)obj.WeaponID ^ (int)obj.Attachments;
    }
}
