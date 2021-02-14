using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ToggleableBindings.JsonNet
{
    public class JsonSerializationBinderException : JsonSerializationException
    {
        public JsonSerializationBinderException() { }

        public JsonSerializationBinderException(string message) : base(message) { }

        public JsonSerializationBinderException(string message, Exception innerException) : base(message, innerException) { }

        public JsonSerializationBinderException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}