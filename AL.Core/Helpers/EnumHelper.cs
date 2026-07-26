#region
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Chaos.Extensions.Common;
#endregion

namespace AL.Core.Helpers;

/// <summary>
///     Provides a set of helper methods for interacting with <see cref="System.Enum" />s.
/// </summary>
public static class EnumHelper
{
    private static readonly Dictionary<Type, Dictionary<string, Enum>> EnumValues = new();
    private static readonly object Sync = new();

    /// <summary>
    ///     Builds (once, cached) the case-insensitive name/<see cref="EnumMemberAttribute" />-alias -&gt; value lookup for an
    ///     enum type. Shared by both
    ///     <c>
    ///         TryParse
    ///     </c>
    ///     overloads so their behaviour cannot drift.
    /// </summary>
    private static Dictionary<string, Enum> GetLookup(Type type)
    {
        // ReSharper disable once InconsistentlySynchronizedField
        if (EnumValues.TryGetValue(type, out var valueLookup))
            return valueLookup;

        lock (Sync)
        {
            if (EnumValues.TryGetValue(type, out valueLookup))
                return valueLookup;

            valueLookup = new Dictionary<string, Enum>(StringComparer.OrdinalIgnoreCase);
            var members = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            foreach (var member in members)
            {
                var raw = member.GetRawConstantValue();

                if (raw == null)
                    continue;

                var value = (Enum)Enum.ToObject(type, raw);
                valueLookup.Add(member.Name, value);

                var enumMember = member.GetCustomAttribute<EnumMemberAttribute>();

                if ((enumMember?.Value == null) || enumMember.Value.EqualsI(member.Name))
                    continue;

                valueLookup.Add(enumMember.Value, value);
            }

            EnumValues[type] = valueLookup;

            return valueLookup;
        }
    }

    /// <summary>
    ///     A helper method for converting an enum to a string, taking into consideration <see cref="EnumMemberAttribute" />s
    ///     if they exist.
    /// </summary>
    /// <typeparam name="T">
    ///     An enum type.
    /// </typeparam>
    /// <param name="value">
    ///     An enum value.
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    ///     <br />
    ///     Returns the value denoted by the <see cref="EnumMemberAttribute" />. Otherwise the value from .ToString
    /// </returns>
    public static string ToString<T>(T value) where T: Enum
    {
        var result = value.ToString();

        var members = typeof(T).GetFields();

        foreach (var member in members)
            if (member.GetValue(value)
                      ?.Equals(value)
                == true)
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
    /// <typeparam name="T">
    ///     An enum type.
    /// </typeparam>
    /// <param name="str">
    ///     A string to parse.
    /// </param>
    /// <param name="result">
    ///     <see cref="Enum" /> value of type <typeparamref name="T" />
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if parsing was successful,
    ///     <c>
    ///         false
    ///     </c>
    ///     otherwise.
    /// </returns>
    public static bool TryParse<T>(string? str, out T? result) where T: Enum
    {
        result = default;

        if (string.IsNullOrEmpty(str))
            return false;

        if (!GetLookup(typeof(T))
                .TryGetValue(str, out var enumValue))
            return false;

        result = (T)enumValue;

        return true;
    }

    /// <summary>
    ///     Non-generic <see cref="TryParse{T}(string?, out T?)" />, for callers that only know the enum type at runtime (e.g.
    ///     a converter over an open generic). Honours <see cref="EnumMemberAttribute" /> the same way.
    /// </summary>
    public static bool TryParse(Type enumType, string? str, out object? result)
    {
        result = null;

        if (string.IsNullOrEmpty(str))
            return false;

        if (!GetLookup(enumType)
                .TryGetValue(str, out var enumValue))
            return false;

        result = enumValue;

        return true;
    }
}