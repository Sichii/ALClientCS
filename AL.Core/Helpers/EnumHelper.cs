using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Chaos.Core.Extensions;

namespace AL.Core.Helpers
{
    /// <summary>
    ///     Provides a set of helper methods for interacting with <see cref="System.Enum" />s.
    /// </summary>
    public static class EnumHelper
    {
        private static readonly Dictionary<Type, Dictionary<string, Enum>> EnumValues = new();
        private static readonly object Sync = new();

        /// <summary>
        ///     A helper method for converting an enum to a string, taking into consideration <see cref="EnumMemberAttribute" />s
        ///     if they exist.
        /// </summary>
        /// <typeparam name="T">An enum type.</typeparam>
        /// <param name="value">An enum value.</param>
        /// <returns>
        ///     <see cref="string" /> <br />
        ///     Returns the value denoted by the <see cref="EnumMemberAttribute" />. Otherwise the value from .ToString
        /// </returns>
        public static string ToString<T>(T value) where T: Enum
        {
            var result = value.ToString();

            var members = typeof(T).GetFields();

            foreach (var member in members)
                if (member.GetValue(value)?.Equals(value) == true)
                {
                    var enumMemberAttr = member.GetCustomAttribute<EnumMemberAttribute>();

                    if (enumMemberAttr?.Value == null)
                        break;

                    return enumMemberAttr.Value;
                }

            return result;
        }

        /// <summary>
        ///     A helper method for parsing an enum from a string, taking into cosideration <see cref="EnumMemberAttribute" />s if
        ///     they exist.
        /// </summary>
        /// <typeparam name="T">An enum type.</typeparam>
        /// <param name="str">A string to parse.</param>
        /// <param name="result"><see cref="Enum" /> value of type <typeparamref name="T" /></param>
        /// <returns><c>true</c> if parsing was successful, <c>false</c> otherwise.</returns>
        public static bool TryParse<T>(string? str, out T? result) where T: Enum
        {
            result = default;

            if (string.IsNullOrEmpty(str))
                return false;

            var type = typeof(T);

            // ReSharper disable once InconsistentlySynchronizedField
            if (!EnumValues.TryGetValue(type, out var valueLookup))
                lock (Sync)
                    if (!EnumValues.TryGetValue(type, out valueLookup))
                    {
                        valueLookup = new Dictionary<string, Enum>(StringComparer.OrdinalIgnoreCase);
                        var members = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                        foreach (var member in members)
                        {
                            var name = member.Name;
                            var value = (T?)member.GetRawConstantValue();

                            if (value == null)
                                continue;

                            valueLookup.Add(name, value);

                            var enumMember = member.GetCustomAttribute<EnumMemberAttribute>();

                            if ((enumMember?.Value == null) || enumMember.Value.EqualsI(name))
                                continue;

                            valueLookup.Add(enumMember.Value, value);
                        }

                        EnumValues[type] = valueLookup;
                    }

            if (!valueLookup.TryGetValue(str, out var enumValue))
                return false;

            result = (T)enumValue;

            return true;
        }
    }
}