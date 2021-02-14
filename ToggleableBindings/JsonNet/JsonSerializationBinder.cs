#nullable enable

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;

namespace ToggleableBindings.JsonNet
{
    // Taken from https://stackoverflow.com/questions/39383098/ignore-missing-types-during-deserialization-of-list
    public class JsonSerializationBinder : SerializationBinder
    {
        public static JsonSerializationBinder Instance { get; } = new(new DefaultSerializationBinder());

        private readonly SerializationBinder _binder;

        public JsonSerializationBinder(SerializationBinder binder)
        {
            if (binder is null)
                throw new ArgumentNullException(nameof(binder));

            _binder = binder;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            try
            {
                return _binder.BindToType(assemblyName, typeName);
            }
            catch (Exception ex)
            {
                throw new JsonSerializationBinderException(ex.Message, ex);
            }
        }
    }
}