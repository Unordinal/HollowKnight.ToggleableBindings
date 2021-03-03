#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ToggleableBindings.Extensions
{
    /// <summary>
    /// Extensions for working with reflection.
    /// </summary>
    internal static class ReflectionExtensions
    {
        public static T? GetCustomAttribute<T>(this MemberInfo element, bool inherit) where T : Attribute
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var attrs = element.GetCustomAttributes<T>(inherit).ToArray();
            if (attrs.Length > 1)
                throw new AmbiguousMatchException("More than one of the requested attributes was found.");

            return attrs.FirstOrDefault();
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo element, bool inherit) where T : Attribute
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return element.GetCustomAttributes(typeof(T), inherit).Cast<T>();
        }

        public static object? GetMemberValue(this Type type, string memberName, object? backingObject)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (memberName == null)
                throw new ArgumentNullException(nameof(memberName));

            var memberInfo = type.GetMember(memberName).FirstOrDefault();
            if (memberInfo == null)
                throw new ArgumentException("The specified member does not exist on the given type.");

            return memberInfo.GetMemberValue(backingObject);
        }

        public static T? GetMemberValue<T>(this Type type, string memberName, object? backingObject)
        {
            return (T)type.GetMemberValue(memberName, backingObject);
        }

        public static object? GetMemberValue(this MemberInfo member, object? backingObject)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            return member switch
            {
                FieldInfo fi => fi.GetValue(backingObject),
                PropertyInfo pi => pi.GetValue(backingObject, null),
                _ => throw new ArgumentException("Cannot get the value of a member info which is not a field or property.")
            };
        }

        public static T? GetMemberValue<T>(this MemberInfo member, object? backingObject)
        {
            return (T)member.GetMemberValue(backingObject);
        }

        public static void SetMemberValue(this MemberInfo member, object? backingObject, object? value, object[]? index = null)
        {
            Type targetType = member switch
            {
                FieldInfo fi => fi.FieldType,
                PropertyInfo pi => pi.PropertyType,
                _ => throw new ArgumentException("Can't set the value of a non-field and non-property member."),
            };

            if (targetType.IsNullable())
                targetType = Nullable.GetUnderlyingType(targetType);

            if (targetType.IsEnum && value != null)
                value = Enum.ToObject(targetType, value);
            else
                value = Convert.ChangeType(value, targetType);

            switch (member)
            {
                case FieldInfo fi:
                    fi.SetValue(backingObject, value);
                    break;

                case PropertyInfo pi:
                    pi.SetValue(backingObject, value, index);
                    break;
            }
        }

        public static Type GetUnderlyingType(this MemberInfo member)
        {
            return member.MemberType switch
            {
                MemberTypes.Event => ((EventInfo)member).EventHandlerType,
                MemberTypes.Field => ((FieldInfo)member).FieldType,
                MemberTypes.Property => ((PropertyInfo)member).PropertyType,
                MemberTypes.Method => ((MethodInfo)member).ReturnType,
                _ => throw new ArgumentException("MemberInfo must be of type Event, Field, Property, or Method.")
            };
        }

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsAssignableTo(this Type type, Type other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return other.IsAssignableFrom(type);
        }
    }
}