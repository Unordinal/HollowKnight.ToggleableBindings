using System.Linq;
using Modding;
using ToggleableBindings.Extensions;
using ToggleableBindings.HKQuickSettings;
using ToggleableBindings.VanillaBindings;
using UnityEngine;
using Vasi;

namespace ToggleableBindings
{
    public class ToggleableBindings : Mod, ITogglableMod
    {
        public static ToggleableBindings Instance { get; private set; }

        public QuickSettings Settings { get; } = new();

        public ToggleableBindings() : base()
        {
            if (Instance is not null)
                return;

            Instance = this;
        }

        public override void Initialize()
        {
            BindingManager.Initialize();
            On.GameManager.Update += GameManager_Update;
        }

        private void GameManager_Update(On.GameManager.orig_Update orig, GameManager self)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                foreach (var binding in BindingManager.RegisteredBindings)
                    Log(binding.Name + " - Applied: " + binding.IsApplied);
                foreach (var evnt in EventRegister.eventRegister)
                    Log(evnt.Key + ": " + string.Join(", ", evnt.Value.Select(e => e.gameObject.ListHierarchy()).ToArray()));
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                var binding = BindingManager.RegisteredBindings.Where(b => b.Name == nameof(NailBinding)).First();
                if (binding.IsApplied)
                    binding.Restore();
                else
                    binding.Apply();
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                var binding = BindingManager.RegisteredBindings.Where(b => b.Name == nameof(ShellBinding)).First();
                if (binding.IsApplied)
                    binding.Restore();
                else
                    binding.Apply();
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                var binding = BindingManager.RegisteredBindings.Where(b => b.Name == nameof(SoulBinding)).First();
                if (binding.IsApplied)
                    binding.Restore();
                else
                    binding.Apply();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                var binding = BindingManager.RegisteredBindings.Where(b => b.Name == nameof(CharmsBinding)).First();
                if (binding.IsApplied)
                    binding.Restore();
                else
                    binding.Apply();
            }
        }

        public void Unload()
        {
            BindingManager.Unload(false);
        }

        public override string GetVersion()
        {
            return VersionUtil.GetVersion<ToggleableBindings>();
        }
    }
}