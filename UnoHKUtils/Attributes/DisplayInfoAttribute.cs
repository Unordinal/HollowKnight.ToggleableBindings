#nullable enable

using System;

namespace UnoHKUtils.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DisplayInfoAttribute : Attribute
    {
        public const string MemberLayout = "{0} = {1}";

        public string? Member { get; }

        public DisplayInfoAttribute(string? member = null)
        {
            Member = member;
        }
    }
}