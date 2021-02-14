#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using ToggleableBindings;
using UnityEngine;

namespace TestNewBinding
{
    public sealed class SpeedBinding : Binding
    {
        [NotNull]
        private Rigidbody2D? _rb2d;

        public SpeedBinding() : base(nameof(SpeedBinding))
        {

        }

        protected override void OnApplied()
        {
            _rb2d = HeroController.instance.GetComponent<Rigidbody2D>();
            On.HeroController.Move += HeroController_Move;
        }

        protected override void OnRestored()
        {
            On.HeroController.Move -= HeroController_Move;
        }

        private void HeroController_Move(On.HeroController.orig_Move orig, HeroController self, float move_direction)
        {
            orig(self, move_direction);
            _rb2d.velocity = new Vector2(_rb2d.velocity.x * 0.5f, _rb2d.velocity.y);
        }
    }
}