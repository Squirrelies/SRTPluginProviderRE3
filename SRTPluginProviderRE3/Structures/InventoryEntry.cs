using SRTPluginBase;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace SRTPluginProviderRE3.Structures
{
    [DebuggerDisplay("{_DebuggerDisplay,nq}")]
    [StructLayout(LayoutKind.Sequential)]
    public struct InventoryEntry : IEquatable<InventoryEntry>
    {
        /// <summary>
        /// Debugger display message.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string _DebuggerDisplay
        {
            get
            {
                if (IsItem)
                    return string.Format("[#{0}] Item {1} Quantity {2}", SlotPosition, ItemID, Quantity);
                else if (IsWeapon)
                    return string.Format("[#{0}] Weapon {1} Quantity {2} Attachments {3}", SlotPosition, WeaponID, Quantity, Attachments);
                else
                    return string.Format("[#{0}] Empty Slot", SlotPosition);
            }
        }

        //internal static readonly byte[] EMPTY_INVENTORY_ITEM = new byte[20] { 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 };
        internal static readonly int[] EMPTY_INVENTORY_ITEM = new int[5] { 0x00000000, unchecked((int)0xFFFFFFFF), 0x00000000, 0x00000000, 0x01000000 };

        // Storage variable.
        public int SlotPosition { get => _slotPosition; set => _slotPosition = value; }
        internal int _slotPosition;
        internal int[] _data;

        internal long _invDataOffset;

        // Accessor properties.
        public ItemEnumeration ItemID => (ItemEnumeration)_data[0];
        public WeaponEnumeration WeaponID => (WeaponEnumeration)_data[1];
        public AttachmentsFlag Attachments => (AttachmentsFlag)_data[2];
        public int Quantity => _data[4];

        public bool IsItem => ItemID != ItemEnumeration.None && (WeaponID == WeaponEnumeration.None || WeaponID == 0);
        public bool IsWeapon => ItemID == ItemEnumeration.None && WeaponID != WeaponEnumeration.None && WeaponID != 0;
        public bool IsEmptySlot => !IsItem && !IsWeapon;

        public bool Equals(InventoryEntry other)
        {
            return _data.SequenceEqual(other._data);
        }

        public override bool Equals(object obj)
        {
            if (obj is InventoryEntry)
                return this.Equals((InventoryEntry)obj);
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static bool operator ==(InventoryEntry obj1, InventoryEntry obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (ReferenceEquals(obj1, null) || ReferenceEquals(obj1._data, null))
                return false;

            if (ReferenceEquals(obj2, null) || ReferenceEquals(obj2._data, null))
                return false;

            return obj1._data.SequenceEqual(obj2._data);
        }

        public static bool operator !=(InventoryEntry obj1, InventoryEntry obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
