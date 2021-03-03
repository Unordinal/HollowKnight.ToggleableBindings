#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

namespace ToggleableBindings.Utility
{
    // Instance Members
    /// <summary>
    /// Represents the result of an operation that returns a value.
    /// </summary>
    /// <typeparam name="T">The type of value returned by the operation.</typeparam>
    public partial class TryResult<T> : TryResult
    {
        [AllowNull]
        private readonly T _value;

        /// <summary>
        /// If the operation was successful, returns the resulting value; otherwise, throws an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public T Value => (IsSuccess) ? _value : throw new InvalidOperationException($"Cannot get the value of an unsuccessful {nameof(TryResult<T>)}.");

        /// <summary>
        /// Creates a new <see cref="TryResult{T}"/> with the specified value or the exception that was caused if unsuccessful.
        /// </summary>
        /// <param name="value">The value to use.</param>
        /// <param name="exception">The exception that was produced by the operation.</param>
        protected TryResult([AllowNull] T value, Exception? exception = null) : base(exception)
        {
            _value = value;
        }

        /// <summary>
        /// If <see cref="TryResult.IsSuccess"/> is <see langword="true"/>, returns the resulting value of the successful operation; otherwise, throws the exception that would have been thrown.
        /// </summary>
        /// <returns>The value returned by the successful operation.</returns>
        public T GetValueOrThrow()
        {
            ThrowIfUnsuccessful();
            return _value;
        }

        /// <summary>
        /// Returns <see cref="TryResult.IsSuccess"/> and gets the value returned by a successful operation or <paramref name="defaultValue"/> if the operation was unsuccessful.
        /// <para/>
        /// The resulting value may be <see langword="null"/> only if the operation either succeeded and returned <see langword="null"/> or failed and <paramref name="defaultValue"/> is <see langword="null"/>.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public bool TryGetValue(out T? result, T? defaultValue = default)
        {
            result = (IsSuccess) ? _value : defaultValue;
            return IsSuccess;
        }
    }

    // Static Members
    public partial class TryResult<T>
    {
        /// <summary>
        /// Implicitly creates a new <see cref="TryResult{T}"/> from the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        public static implicit operator TryResult<T>([AllowNull] T value) => new(value);

        /// <summary>
        /// Implicitly creates a new <see cref="TryResult{T}"/> from the given exception.
        /// </summary>
        /// <param name="value">The value.</param>
        public static implicit operator TryResult<T>(Exception value) => new(default, value);
    }
}