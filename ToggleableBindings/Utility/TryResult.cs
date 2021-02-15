#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

namespace ToggleableBindings.Utility
{
    /*
     * Normally this type would use ExceptionDispatchInfo, but we're on .NET 3.5 
     * which doesn't have that and backporting it would be a real pain.
     */

    // Instance Members
    /// <summary>
    /// Represents the result of an operation.
    /// </summary>
    public partial class TryResult
    {
        /// <summary>
        /// Indicates whether the operation was successful.
        /// </summary>
        [MemberNotNullWhen(false, nameof(Exception))]
        public bool IsSuccess => Exception is null;

        /// <summary>
        /// If the operation was unsuccessful, contains an <see cref="System.Exception"/> that would have been thrown.
        /// </summary>
        public Exception? Exception { get; }

        protected TryResult(Exception? exception = null)
        {
            Exception = exception;
        }

        /// <summary>
        /// Throws the contained exception if <see cref="IsSuccess"/> is <see langword="false"/>.
        /// </summary>
        public void ThrowIfUnsuccessful()
        {
            if (!IsSuccess)
                throw Exception;
        }

        public override string ToString()
        {
            string output = IsSuccess.ToString();
            if (!IsSuccess)
                output += ": " + Exception.ToString();
            return output;
        }
    }

    // Static Members
    public partial class TryResult
    {
        /// <summary>
        /// Returns a <see cref="TryResult"/> indicating a successful operation.
        /// </summary>
        public static TryResult Success { get; } = new TryResult();

        public static implicit operator TryResult(Exception value) => new(value);
    }
}