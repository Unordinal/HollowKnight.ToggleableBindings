#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HutongGames.PlayMaker;
using MonoMod.Utils;
using UnoHKUtils.Attributes;
using UnoHKUtils.Extensions;

namespace UnoHKUtils
{
    internal static class MiscUtil
    {
        public static void PrintTree<T>(T root, Func<T, string> nodeLabel, Func<T, IEnumerable<T>> childrenOf)
        {
            List<T> firstList = new() { root };
            List<List<T>> childListStack = new() { firstList };

            while (childListStack.Count > 0)
            {
                List<T> childStack = childListStack[^1];
                if (childStack.Count == 0)
                    childListStack.RemoveAt(childListStack.Count - 1);
                else
                {
                    root = childStack[0];
                    childStack.RemoveAt(0);

                    string indent = "";
                    for (int i = 0; i < childListStack.Count - 1; i++)
                        indent += (childListStack[i].Count > 0) ? "|  " : "   ";

                    UnoHKUtils.Instance.Log(indent + "+- " + nodeLabel(root));
                    var children = childrenOf(root);
                    if (children.Any())
                        childListStack.Add(new List<T>(children));
                }
            }
        }

        public static string GetTooltipText(this Type type)
        {
            TooltipAttribute tooltip = (TooltipAttribute)type
                .GetCustomAttributes(typeof(TooltipAttribute), false)
                .FirstOrDefault();

            if (tooltip is not null && tooltip.Text.Length > 0)
                return tooltip.Text;

            return string.Empty;
        }
        
        public static string GetTooltipText(this MemberInfo mi)
        {
            TooltipAttribute tooltip = (TooltipAttribute)mi
                .GetCustomAttributes(typeof(TooltipAttribute), false)
                .FirstOrDefault();

            if (tooltip is not null && tooltip.Text.Length > 0)
                return tooltip.Text;

            return string.Empty;
        }

        public static IEnumerable<(string Name, string Tooltip, T Value, Type Type)> GetAllFieldsOfType<T>(object obj)
        {
            if (obj is null)
                yield break;

            Type typeOfT = typeof(T);
            Type typeOfObj = obj.GetType();

            FieldInfo[] fieldInfos = typeOfObj.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var fi in fieldInfos)
            {
                if (typeOfT.IsAssignableFrom(fi.FieldType))
                    yield return (fi.Name, fi.GetTooltipText(), (T)fi.GetValue(obj), fi.FieldType);
            }
        }

        public static string OrNullString(this object? value)
        {
            return value?.ToString() ?? "<null>";
        }

        public static string DisplayInfo<T>(this T? obj, MemberTypes types = MemberTypes.Field | MemberTypes.Property, BindingFlags flags = ReflectionExtensions.AllFlags)
        {
            Type type = typeof(T);

            Dictionary<string, string> members = new();

            var attrs = type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(mi => (mi.MemberType & types) != 0)
                .Select(mi => (mi, mi.GetCustomAttribute<DisplayInfoAttribute>(false)))
                .Where(i => i.Item2 is not null);
            if (attrs.Any())
            {
                foreach (var (mi, attr) in attrs)
                {
                    string name = mi.Name;
                    object? value = mi.GetMemberValue(obj);
                    if (attr.Member is not null && value is not null)
                    {
                        var memType = mi.GetUnderlyingType();
                        var foundMember = memType.GetMemberValue(attr.Member, value);
                        if (foundMember is not null)
                            value = foundMember;
                    }

                    members[name] = value.OrNullString();
                }
            }
            else
                members = type.GetMembers().Where(mi => (mi.MemberType & types) != 0).ToDictionary(mi => mi.Name, mi => mi.GetMemberValue(obj).OrNullString());
            
            string output = $"<{type.Name}>";
            var memberDisplay = members.Select(kv => string.Format(DisplayInfoAttribute.MemberLayout, kv.Key, kv.Value));
            if (memberDisplay.Any())
                output += " " + "{ " + StringExtensions.Join(", ", memberDisplay!) + " }";

            return output;
        }
    }
}
