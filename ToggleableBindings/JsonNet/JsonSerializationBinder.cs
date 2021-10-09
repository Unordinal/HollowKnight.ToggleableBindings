#nullable enable

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;

namespace ToggleableBindings.JsonNet
{
    // Taken from https://stackoverflow.com/questions/39383098/ignore-missing-types-during-deserialization-of-list
    internal class JsonSerializationBinder : ISerializationBinder
    {
        public static JsonSerializationBinder Instance { get; } = new();

        private readonly DefaultSerializationBinder _defaultBinder = new();

        public Type BindToType(string? assemblyName, string typeName)
        {
            try
            {
                return _defaultBinder.BindToType(assemblyName, typeName);
                //return _binder.BindToType(assemblyName, typeName);
            }
            catch (Exception ex)
            {
                throw new JsonSerializationBinderException(ex.Message, ex);
            }
        }

        public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            _defaultBinder.BindToName(serializedType, out assemblyName, out typeName);
        }
    }
}