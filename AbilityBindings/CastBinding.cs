using ToggleableBindings;

namespace AbilityBindings
{
    public class CastBinding : Binding
    {
        public CastBinding() : base("Cast") { }

        protected override void OnApplied()
        {
            On.HeroController.CanCast += HeroController_CanCast;
        }

        protected override void OnRestored()
        {
            On.HeroController.CanCast -= HeroController_CanCast;
        }

        private bool HeroController_CanCast(On.HeroController.orig_CanCast orig, HeroController self)
        {
            return false;
        }
    }
}