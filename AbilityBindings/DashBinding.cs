#nullable enable

using ToggleableBindings;
using UnityEngine;

namespace AbilityBindings
{
    public class DashBinding : Binding
    {
        public override Sprite? DefaultSprite { get; }

        public override Sprite? SelectedSprite { get; }

        public DashBinding() : base("Dash") { }

        protected override void OnApplied()
        {
            On.HeroController.CanDash += HeroController_CanDash;
        }

        protected override void OnRestored()
        {
            On.HeroController.CanDash -= HeroController_CanDash;
        }

        private bool HeroController_CanDash(On.HeroController.orig_CanDash orig, HeroController self)
        {
            return false;
        }
    }
}