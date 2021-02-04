using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HutongGames.PlayMaker;

namespace UnoHKUtils.FsmUtils
{
    public static class FsmStateActionExtensions
    {
        public static void LogInfo(this FsmStateAction stateAction, int indent)
        {
            Type type = stateAction.GetType();

            FsmUtil.Log($"'{type.Name}' // {type.GetTooltipText()}", indent);

            indent += FsmUtil.IndentBy;
            
            var fields = MiscUtil.GetAllFieldsOfType<object>(stateAction);
            foreach (var (Name, Tooltip, Value, Type) in fields)
            {
                string valueStr = Value?.ToStringExt() ?? "<null>";
                string output = $"{Type.Name} {Name} = {valueStr}";

                if (!string.IsNullOrEmpty(Tooltip))
                    output += " // " + Regex.Replace(Tooltip, @"(\r\n|\r|\n)+", " ");
                FsmUtil.Log(output, indent);
            }
            FsmUtil.LogEmpty();
        }
    }
}
