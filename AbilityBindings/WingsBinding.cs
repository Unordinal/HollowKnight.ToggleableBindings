#nullable enable

using ToggleableBindings;
using UnityEngine;

namespace AbilityBindings
{
    public class WingsBinding : Binding
    {
        public WingsBinding() : base("Wings") { }

        protected override void OnApplied()
        {
            On.HeroController.CanDoubleJump += HeroController_CanDoubleJump;
        }

        protected override void OnRestored()
        {
            On.HeroController.CanDoubleJump -= HeroController_CanDoubleJump;
        }

        private bool HeroController_CanDoubleJump(On.HeroController.orig_CanDoubleJump orig, HeroController self)
        {
            return false;
        }
    }
}