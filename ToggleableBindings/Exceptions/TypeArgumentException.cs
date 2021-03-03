#nullable enable

using System;
using System.Runtime.Serialization;

namespace ToggleableBindings.Exceptions
{
    /// <summary>
    /// The exception that is thrown when one of the type arguments provided to a method is not valid.
    /// </summary>
    public class TypeArgumentException : Exception
    {
        private const string DefaultMessage = "An invalid type argument was specified.";
        private const string TypeParamName_Name = "(Type parameter '{0}')";

        /// <inheritdoc/>
        public override string Message
        {
            get
            {
                string output = base.Message;
                if (!string.IsNullOrEmpty(TypeParamName))
                    output += " " + string.Format(TypeParamName_Name, TypeParamName);

                return output;
            }
        }

        /// <summary>
        /// Gets the name of the type parameter that caused this exception.
        /// </summary>
        public string? TypeParamName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeArgumentException"/> class.
        /// </summary>
        public TypeArgumentException() : this(DefaultMessage) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeArgumentException"/> class
        /// with a specified error message.
        /// </summary>
        public TypeArgumentException(string? message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeArgumentException"/> class
        /// with a specified error message and the name of the type parameter that causes this exception.
        /// </summary>
        public TypeArgumentException(string? message, string? typeParamName) : base(message)
        {
            TypeParamName = typeParamName;
        }

        /// /// <summary>
        /// Initializes a new instance of the <see cref="TypeArgumentException"/> class
        /// with a specified error message and
        /// a reference to the inner exception that is the cause of this exception.
        /// </summary>
        public TypeArgumentException(string? message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeArgumentException"/> class
        /// with serialized data.
        /// </summary>
        /// <inheritdoc/>
        protected TypeArgumentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}