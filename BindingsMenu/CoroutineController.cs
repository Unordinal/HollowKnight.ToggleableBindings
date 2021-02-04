#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TogglableBindings
{
    public class CoroutineController : MonoBehaviour
    {
        private static CoroutineController Instance { get; set; } = null!;

        private static readonly Dictionary<string, IEnumerator> _idRoutines = new(25);

        static CoroutineController()
        {
            Initialize();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (Instance is null)
            {
                Instance = new GameObject($"#{nameof(CoroutineController)}").AddComponent<CoroutineController>();
                DontDestroyOnLoad(Instance);
            }
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