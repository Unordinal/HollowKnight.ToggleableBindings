#nullable enable

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using ToggleableBindings.Debugging.Exceptions;
using UnityEngine;

namespace ToggleableBindings.Debugging
{
    public static class Assert
    {
        private const string AssertionFailed = "{0} failed.";
        private const string EqualFailMsg = "{0} Expected: <{1}>. Actual: <{2}>.";

        [Conditional("DEBUG")]
        public static void IsTrue([DoesNotReturnIf(false)] bool condition, string? message = null, params object[] parameters)
        {
            if (!condition)
                HandleFail("Assert.IsTrue", message, parameters);
        }

        [Conditional("DEBUG")]
        public static void IsFalse([DoesNotReturnIf(true)] bool condition, string? message = null, params object[] parameters)
        {
            if (condition)
                HandleFail("Assert.IsFalse", message, parameters);
        }

        [Conditional("DEBUG")]
        public static void AreEqual(object? expected, object? actual, string? message = null, params object[] parameters)
        {
            if (!Equals(expected, actual))
            {
                string finalMessage = string.Format
                (
                    CultureInfo.CurrentCulture,
                    EqualFailMsg,
                    message != null ? ReplaceNulls(message) : string.Empty,
                    ReplaceNulls(expected),
                    ReplaceNulls(actual)
                );

                HandleFail("Assert.AreEqual", finalMessage, parameters);
            }
        }

        [Conditional("DEBUG")]
        public static void AreNotEqual(object? expected, object? actual, string? message = null, params object[] parameters)
        {
            if (Equals(expected, actual))
            {
                string finalMessage = string.Format
                (
                    CultureInfo.CurrentCulture,
                    EqualFailMsg,
                    message != null ? ReplaceNulls(message) : string.Empty,
                    ReplaceNulls(expected),
                    ReplaceNulls(actual)
                );

                HandleFail("Assert.AreNotEqual", finalMessage, parameters);
            }
        }

        public static void IsNull([MaybeNull] object? value, string? message = null, params object[] parameters)
        {
            if (!ObjectIsFakeOrRealNull(value))
                HandleFail("Assert.IsNull", message, parameters);
        }

#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.
        public static void IsNotNull([NotNull] object? value, string? message = null, params object[] parameters)
        {
            if (ObjectIsFakeOrRealNull(value))
                HandleFail("Assert.IsNotNull", message, parameters);
        }
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.

        private static void HandleFail(string assertName, string? message, params object[] parameters)
        {
            string finalMessage = string.Empty;
            if (!string.IsNullOrEmpty(message))
            {
                finalMessage = ReplaceNulls(message!);

                if (parameters != null)
                    finalMessage = string.Format(CultureInfo.CurrentCulture, finalMessage, parameters);
            }

            string exMessage = string.Format(CultureInfo.CurrentCulture, AssertionFailed, assertName, finalMessage);
            throw new AssertFailedException(exMessage);
        }

        private static string ReplaceNulls(object? input)
        {
            if (input == null)
                return "(null)";

            string inputStr = input.ToString();
            if (inputStr == null)
                return "(null_ToString)";

            if (string.IsNullOrEmpty(inputStr))
                return inputStr;

            return inputStr.Replace("\0", "\\0");
        }

        private static bool ObjectIsFakeOrRealNull(object? value)
        {
            if ((value is Object unityObject && unityObject == null) || value == null)
                return true;

            return false;
        }
    }
}