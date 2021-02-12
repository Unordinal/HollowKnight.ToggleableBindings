using MonoMod.RuntimeDetour;

namespace ToggleableBindings
{
    public static class TBConstants
    {
        public static HookConfig HookManualApply { get; } = new() { ManualApply = true };
    }
}