#nullable enable

using ToggleableBindings;
using UnityEngine;

namespace AbilityBindings
{
    public class ClawBinding : Binding
    {
        public ClawBinding() : base("Claw") { }

        protected override void OnApplied()
        {
            On.HeroController.CanWallJump += HeroController_CanWallJump;
            On.HeroController.CanWallSlide += HeroController_CanWallSlide;
        }

        protected override void OnRestored()
        {
            On.HeroController.CanWallJump -= HeroController_CanWallJump;
            On.HeroController.CanWallSlide -= HeroController_CanWallSlide;
        }

        private bool HeroController_CanWallJump(On.HeroController.orig_CanWallJump orig, HeroController self)
        {
            return false;
        }

        private bool HeroController_CanWallSlide(On.HeroController.orig_CanWallSlide orig, HeroController self)
        {
            return false;
        }
    }
}