using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ToggleableBindings.HKQuickSettings.JsonNet
{
    // Taken from https://stackoverflow.com/a/31732029/
    public class GetOnlyContractResolver : DefaultContractResolver
    {
        public static GetOnlyContractResolver Instance { get; } = new();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (property is not null && property.Writable)
            {
                var attrs = property.AttributeProvider.GetAttributes(typeof(JsonGetOnlyPropertyAttribute), true);
                if (attrs is not null && attrs.Count > 0)
                    property.Writable = false;
            }

            return property;
        }
    }
}