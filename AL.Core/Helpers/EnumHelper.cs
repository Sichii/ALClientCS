using System;
using System.Reflection;
using System.Runtime.Serialization;
using Chaos.Core.Extensions;

namespace AL.Core.Helpers
{
    public static class EnumHelper
    {
        public static bool TryParse<T>(string str, out T result) where T: struct
        {
            result = default;

            if (str == null)
                return false;

            if (Enum.TryParse(str, true, out result))
                return true;

            var members = typeof(T).GetFields();

            foreach (var member in members)
            {
                var enumMember = member.GetCustomAttribute<EnumMemberAttribute>();

                if (enumMember == null || !enumMember.Value.EqualsI(str))
                    continue;

                var final = member.GetValue(null);
                result = (T?) final ?? default;
                return true;
            }

            return false;
        }

        public static string ToString<T>(T value) where T: Enum
        {
            var result = value.ToString();
            
            var members = typeof(T).GetFields();

            foreach (var member in members)
            {
                if (member.GetValue(value)?.Equals(value) == true)
                {
                    var enumMemberAttr = member.GetCustomAttribute<EnumMemberAttribute>();

                    if (enumMemberAttr == null)
                        break;

                    return enumMemberAttr.Value;
                }
            }

            return result;
        }
    }
}