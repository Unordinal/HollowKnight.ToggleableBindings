using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InControl;
using Modding;
using ToggleableBindings;
using Vasi;

namespace TestNewBinding
{
    public class TestNewBinding : Mod, ITogglableMod
    {
        public override string GetVersion() => VersionUtil.GetVersion<TestNewBinding>();

        public override int LoadPriority() => 20;

        public override void Initialize()
        {
            BindingManager.RegisterBinding<SpeedBinding>();
        }

        public void Unload()
        {

        }
    }
}
