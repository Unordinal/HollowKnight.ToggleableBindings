using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;

namespace UnoHKUtils
{
    public static class ToStringExtensions
    {
        public static string ToStringExt(this object obj)
        {
            return obj switch
            {
                FsmGameObject fsmGO => fsmGO.ToStringExt(),
                FsmOwnerDefault fsmOD => fsmOD.ToStringExt(),
                FsmEvent fsmEvent => fsmEvent.ToStringExt(),
                NamedVariable namedVar => namedVar.ToStringExt(),
                _ => $"'{obj?.ToString() ?? "<null>"}' ({obj?.GetType().Name})",
            };
        }

        public static string ToStringExt(this FsmGameObject value)
        {
            return TypeAndInfo(nameof(FsmGameObject), InfoList(("Name", value.Name), ("ValueName", value.Value?.name), ("Value", value.Value)));
        }

        public static string ToStringExt(this FsmOwnerDefault value)
        {
            return TypeAndInfo(nameof(FsmOwnerDefault), InfoList(("OwnerOption", value.OwnerOption), ("GameObject", value.GameObject)));
        }

        public static string ToStringExt(this FsmEvent value)
        {
            return TypeAndInfo(nameof(FsmEvent), InfoList(("Name", value.Name), ("Path", value.Path), ("IsGlobal", value.IsGlobal)));
        }

        public static string ToStringExt(this NamedVariable value)
        {
            return TypeAndInfo(nameof(NamedVariable), InfoList(("Name", value.Name), ("VariableType", value.VariableType), ("RawValue", value.RawValue)));
        }

        private static string TypeAndInfo(string typeName, string info)
        {
            return typeName + " " + info;
        }

        private static string InfoList(params (string Name, object Value)[] props)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{ ");
            for (int i = 0; i < props.Length; i++)
            {
                var (Name, Value) = props[i];
                sb.Append($"{Name} = {Value?.ToStringExt() ?? "<null>"}");
                if (i != props.Length - 1)
                    sb.Append(", ");
            }
            sb.Append(" }");

            return sb.ToString();
        }
    }
}
