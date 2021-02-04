#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TogglableBindings.Extensions;
using UnityEngine;

namespace TogglableBindings
{
    public readonly struct CoroutineBuilder
    {
        private readonly IEnumerable<Instruction> _instructions;

        public static CoroutineBuilder New { get; } = new();

        public IEnumerable<Instruction> Instructions
        {
            get => _instructions ?? Enumerable.Empty<Instruction>();
            init => _instructions = value;
        }

        public CoroutineBuilder(IEnumerable<Instruction> instructions)
        {
            _instructions = instructions;
        }

        public CoroutineBuilder WithYield(params object?[]? toYield)
        {
            var instrs = Instructions;
            instrs = (toYield is not null)
                ? instrs.Concat(toYield.Select(o => new Instruction(o)))
                : instrs.Concat(Instruction.YieldNull);

            return new CoroutineBuilder(instrs);
        }

        public CoroutineBuilder WithAction(params Action?[]? actions)
        {
            var instrs = Instructions;
            if (actions is not null)
                instrs = instrs.Concat(actions.Select(a => new Instruction(a)));
            else
                instrs = instrs.Concat(Instruction.YieldNull);

            return new CoroutineBuilder(instrs);
        }

        public Coroutine Start()
        {
            return CoroutineController.Start(AsCoroutine());
        }

        public Coroutine Start(string id)
        {
            return CoroutineController.Start(AsCoroutine(), id);
        }

        public IEnumerator AsCoroutine()
        {
            foreach (var instr in Instructions)
            {
                if (instr.IsYield)
                    yield return instr.Yield;
                else
                    instr.Action?.Invoke();
            }
        }

        /// <summary>
        /// A simple representation of a method instruction that can either be an <see cref="System.Action"/> to invoke or an object to yield return.
        /// </summary>
        public readonly struct Instruction
        {
            /// <summary>
            /// Gets an <see cref="Instruction"/> representing 'yield return <see langword="null"/>'.
            /// </summary>
            public static Instruction YieldNull { get; } = new();

            /// <summary>
            /// If not <see langword="null"/>, the action to be invoked.
            /// </summary>
            public Action? Action { get; }

            /// <summary>
            /// If <see cref="Action"/> is <see langword="null"/>, the <see cref="object"/> to yield return. Can be <see langword="null"/>.
            /// </summary>
            public object? Yield { get; }

            /// <summary>
            /// Gets whether this should be a Yield instruction.
            /// </summary>
            /// <returns><see langword="true"/> if <see cref="Action"/> is <see langword="null"/>.</returns>
            public bool IsYield => Action is null;

            public Instruction(Action? action) : this()
            {
                Action = action;
            }

            public Instruction(object? yield) : this()
            {
                Yield = yield;
            }

            public static implicit operator Instruction(Action? value) => new(value);

            public static implicit operator Instruction(YieldInstruction value) => new(value);

            public static implicit operator Instruction(CustomYieldInstruction value) => new(value);
        }
    }
}