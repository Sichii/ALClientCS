namespace AL.Core.Attributes;

/// <summary>
///     Marks a property that a shallow merge must not copy, even though it is both readable and writable.
/// </summary>
/// <remarks>
///     A private setter on a base of the merged type already hides a property from the merge; this covers a property
///     declared on the merged type itself, and a setter later widened to protected or public.
/// </remarks>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Property)]
public sealed class ShallowMergeIgnoreAttribute : Attribute;