using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SRTPluginProviderRE3.Structures
{
    [DebuggerDisplay("{_DebuggerDisplay,nq}")]
    [StructLayout(LayoutKind.Sequential)]
    public struct EnemyHP
    {
        /// <summary>
        /// Debugger display message.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string _DebuggerDisplay
        {
            get
            {
                if (IsAlive)
                    return string.Format("{0} / {1} ({2:P1})", CurrentHP, MaximumHP, Percentage);
                else
                    return "DEAD / DEAD (0%)";
            }
        }

        public int MaximumHP { get => _maximumHP; set => _maximumHP = value; }
        internal int _maximumHP;

        public int CurrentHP { get => _currentHP; set => _currentHP = value; }
        internal int _currentHP;

        public bool IsAlive => MaximumHP > 0 && CurrentHP > 0 && CurrentHP <= MaximumHP;
        public float Percentage => ((IsAlive) ? (float)CurrentHP / (float)MaximumHP : 0f);
    }
}
