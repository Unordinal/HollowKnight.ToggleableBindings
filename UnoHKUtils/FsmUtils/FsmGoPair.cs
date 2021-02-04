#nullable enable

using HutongGames.PlayMaker;
using UnityEngine;
using UnoHKUtils.Attributes;

namespace UnoHKUtils.FsmUtils
{
    /// <summary>
    /// Represents a <see cref="UnityEngine.GameObject"/> paired with its <see cref="HutongGames.PlayMaker.Fsm"/> objects.
    /// </summary>
    public class FsmGOPair
    {
        [DisplayInfo(nameof(UnityEngine.GameObject.name))]
        public GameObject? GameObject { get; init; }

        [DisplayInfo(nameof(HutongGames.PlayMaker.Fsm.Name))]
        public Fsm? Fsm { get; init; }

        public override string ToString()
        {
            return this.DisplayInfo();
        }
    }
}