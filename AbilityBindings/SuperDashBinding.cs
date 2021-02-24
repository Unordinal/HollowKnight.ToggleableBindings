#nullable enable

using ToggleableBindings;
using UnityEngine;

namespace AbilityBindings
{
    public class SuperDashBinding : Binding
    {
        public override Sprite? DefaultSprite { get; }

        public override Sprite? SelectedSprite { get; }

        public SuperDashBinding() : base("Super Dash") { }

        protected override void OnApplied()
        {
            On.HeroController.CanSuperDash += HeroController_CanSuperDash;
        }

        protected override void OnRestored()
        {
            On.HeroController.CanSuperDash -= HeroController_CanSuperDash;
        }

        private bool HeroController_CanSuperDash(On.HeroController.orig_CanSuperDash orig, HeroController self)
        {
            return false;
        }
    }
}