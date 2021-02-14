using System;
using System.Runtime.Serialization;

namespace ToggleableBindings.Exceptions
{
    /// <summary>
    /// The exception that is thrown when one of the type arguments provided to a method refers to a type that is abstract.
    /// </summary>
    public class TypeArgumentAbstractException : TypeArgumentException
    {
        private const string DefaultMessage = "The specified type parameter should not reference an abstract type.";

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeArgumentAbstractException"/> class.
        /// </summary>
        public TypeArgumentAbstractException() : this(DefaultMessage) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeArgumentAbstractException"/> class
        /// with a specified error message.
        /// </summary>
        public TypeArgumentAbstractException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeArgumentAbstractException"/> class
        /// with a specified error message and the name of the type parameter that causes this exception.
        /// </summary>
        public TypeArgumentAbstractException(string message, string typeParamName) : base(message, typeParamName) { }

        /// /// <summary>
        /// Initializes a new instance of the <see cref="TypeArgumentAbstractException"/> class
        /// with a specified error message and
        /// a reference to the inner exception that is the cause of this exception.
        /// </summary>
        public TypeArgumentAbstractException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeArgumentAbstractException"/> class
        /// with serialized data.
        /// </summary>
        /// <inheritdoc/>
        protected TypeArgumentAbstractException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}