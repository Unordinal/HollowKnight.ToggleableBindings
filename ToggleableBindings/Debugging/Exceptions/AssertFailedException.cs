#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;

namespace ToggleableBindings.Debugging.Exceptions
{
    public class AssertFailedException : Exception
    {
        public AssertFailedException() : base() { }

        public AssertFailedException(string message) : base(message) { }

        public AssertFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}