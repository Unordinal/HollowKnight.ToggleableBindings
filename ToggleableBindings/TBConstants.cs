using MonoMod.RuntimeDetour;

namespace ToggleableBindings
{
    internal static class TBConstants
    {
        public static HookConfig HookManualApply { get; } = new() { ManualApply = true };
    }
}