using System;

namespace AL.Core.Json.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class JsonArrayIndexAttribute : Attribute
    {
        public int Index { get; }
        public JsonArrayIndexAttribute(int index) => Index = index;
    }
}