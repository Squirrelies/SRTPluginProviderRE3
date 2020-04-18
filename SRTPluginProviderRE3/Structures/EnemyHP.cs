using System.Diagnostics;

namespace SRTPluginProviderRE3.Structures
{
    [DebuggerDisplay("{_DebuggerDisplay,nq}")]
    public class EnemyHP
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

        public int MaximumHP { get; set; }
        public int CurrentHP { get; set; }
        public bool IsAlive => MaximumHP > 0 && CurrentHP > 0 && CurrentHP <= MaximumHP;
        public float Percentage => ((IsAlive) ? (float)CurrentHP / (float)MaximumHP : 0f);

        public EnemyHP()
        {
            this.MaximumHP = 0;
            this.CurrentHP = 0;
        }
    }
}
