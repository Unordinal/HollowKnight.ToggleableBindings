using System;
using MonoMod.RuntimeDetour;

namespace TogglableBindings
{
    public static class ModConstants
    {
        public static HookConfig HookManualApply { get; } = new() { ManualApply = true };

        public static int BoundShellMaxHealth => 4;

        public static int BoundNailDamage
        {
            get
            {
                int currDamage = PlayerData.instance.nailDamage;
                return (int)Math.Min(currDamage * 0.8, 13);
            }
        }
    }
}