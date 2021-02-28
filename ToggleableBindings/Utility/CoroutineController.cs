#nullable enable
#pragma warning disable UNT0006 // Incorrect message signature

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace ToggleableBindings.Utility
{
    public class CoroutineController : MonoBehaviour
    {
        [NotNull, DisallowNull]
        private static CoroutineController? Instance { get; set; }

        private static readonly Dictionary<string, IEnumerator> _idRoutines = new();

        static CoroutineController()
        {
            Initialize();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (Instance != null)
            {
                ToggleableBindings.Instance.LogError($"Tried to create a new {nameof(CoroutineController)} when one already exists!");
                return;
            }

            var container = ObjectFactory.Create(nameof(CoroutineController), null, InstanceFlags.DontDestroyOnLoad);
            Instance = container.AddComponent<CoroutineController>();
        }

        public static Coroutine Start(IEnumerator routine)
        {
            return Instance.StartCoroutine(routine);
        }

        public static Coroutine Start(IEnumerator routine, string id)
        {
            if (_idRoutines.ContainsKey(id))
                Instance.StopCoroutine(_idRoutines[id]);

            var coroutine = Instance.StartCoroutine(routine);
            _idRoutines[id] = routine;
            return coroutine;
        }

        public static void Stop(IEnumerator routine)
        {
            Instance.StopCoroutine(routine);
        }

        public static void Stop(Coroutine routine)
        {
            Instance.StopCoroutine(routine);
        }

        public static void Stop(string id)
        {
            if (_idRoutines.TryGetValue(id, out var routine))
            {
                Instance.StopCoroutine(routine);
                _idRoutines.Remove(id);
            }
        }

        public static IEnumerator ToCoroutine(Action? action, IEnumerable<object?>? yieldBefore = null, IEnumerable<object?>? yieldAfter = null)
        {
            return CoroutineBuilder.New
                .WithYield(yieldBefore)
                .WithAction(action)
                .WithYield(yieldAfter)
                .AsCoroutine();
        }
    }
}