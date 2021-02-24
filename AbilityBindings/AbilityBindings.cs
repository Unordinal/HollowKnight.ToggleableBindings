using Modding;
using ToggleableBindings;
using Vasi;

namespace AbilityBindings
{
    public class AbilityBindings : Mod, ITogglableMod
    {
        public override string GetVersion() => VersionUtil.GetVersion<AbilityBindings>();

        public override void Initialize()
        {
            BindingManager.RegisterBinding<DashBinding>();
            BindingManager.RegisterBinding<SuperDashBinding>();
            BindingManager.RegisterBinding<ClawBinding>();
            BindingManager.RegisterBinding<WingsBinding>();
        }

        public void Unload()
        {
            BindingManager.DeregisterBinding<DashBinding>();
            BindingManager.DeregisterBinding<SuperDashBinding>();
            BindingManager.DeregisterBinding<ClawBinding>();
            BindingManager.DeregisterBinding<WingsBinding>();
        }
    }
}