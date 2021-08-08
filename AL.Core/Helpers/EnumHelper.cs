using System;
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
        /// <param name="result"><see cref="Enum" /> value of type <see cref="T" /></param>
        /// <returns><c>true</c> if parsing was successful, <c>false</c> otherwise.</returns>
        public static bool TryParse<T>(string? str, out T result) where T: struct
        {
            result = default;

            if (string.IsNullOrEmpty(str))
                return false;

            if (Enum.TryParse(str, true, out result))
                return true;

            var members = typeof(T).GetFields();

            foreach (var member in members)
            {
                var enumMember = member.GetCustomAttribute<EnumMemberAttribute>();

                if ((enumMember == null) || !enumMember.Value.EqualsI(str))
                    continue;

                var final = member.GetValue(null);
                result = (T?)final ?? default;
                return true;
            }

            return false;
        }
    }
}