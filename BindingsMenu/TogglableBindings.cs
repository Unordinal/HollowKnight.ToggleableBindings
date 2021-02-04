using System.Linq;
using System.Reflection;
using Modding;
using TogglableBindings.Extensions;
using TogglableBindings.HKQuickSettings;
using TogglableBindings.VanillaBindings;
using UnityEngine;
using Vasi;

namespace TogglableBindings
{
    public class TogglableBindings : Mod, ITogglableMod
    {
        public static TogglableBindings Instance { get; private set; }

        public QuickSettings Settings { get; } = new();

        public TogglableBindings() : base()
        {
            if (Instance is not null)
                return;

            Instance = this;
        }

        public override void Initialize()
        {
            BindingsManager.Initialize();
            On.GameManager.Update += GameManager_Update;
        }

        private void GameManager_Update(On.GameManager.orig_Update orig, GameManager self)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                foreach (var binding in BindingsManager.RegisteredBindings)
                    Log(binding.Name + " - Applied: " + binding.IsApplied);
                foreach (var evnt in EventRegister.eventRegister)
                    Log(evnt.Key + ": " + string.Join(", ", evnt.Value.Select(e => e.gameObject.ListHierarchy()).ToArray()));
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                var binding = BindingsManager.RegisteredBindings.Where(b => b.Name == nameof(NailBinding)).First();
                if (binding.IsApplied)
                    binding.Restore();
                else
                    binding.Apply();
            }
            
            if (Input.GetKeyDown(KeyCode.J))
            {
                var binding = BindingsManager.RegisteredBindings.Where(b => b.Name == nameof(ShellBinding)).First();
                if (binding.IsApplied)
                    binding.Restore();
                else
                    binding.Apply();
            }
            
            if (Input.GetKeyDown(KeyCode.K))
            {
                var binding = BindingsManager.RegisteredBindings.Where(b => b.Name == nameof(SoulBinding)).First();
                if (binding.IsApplied)
                    binding.Restore();
                else
                    binding.Apply();
            }
            
            if (Input.GetKeyDown(KeyCode.L))
            {
                var binding = BindingsManager.RegisteredBindings.Where(b => b.Name == nameof(CharmsBinding)).First();
                if (binding.IsApplied)
                    binding.Restore();
                else
                    binding.Apply();
            }
        }

        public void Unload()
        {
            BindingsManager.Unload(false);
        }

        public override string GetVersion()
        {
            return VersionUtil.GetVersion<TogglableBindings>();
        }
    }
}