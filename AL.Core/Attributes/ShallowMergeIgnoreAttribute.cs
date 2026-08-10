namespace AL.Core.Attributes;

/// <summary>
///     Marks a property that a shallow merge must not copy, even though it is both readable and writable.
/// </summary>
/// <remarks>
///     <c>
///         AL.Client.Helpers.ShallowMerge&lt;T&gt;
///     </c>
///     selects properties on
///     <c>
///         CanRead &amp;&amp; CanWrite
///     </c>
///     over public and non-public accessors alike. It reflects
///     <c>
///         typeof(T)
///     </c>
///     , and a private setter declared on a <i>base</i> of T is not inherited, so from T's view
///     <c>
///         CanWrite
///     </c>
///     is already false - which is what actually excludes the movement block from the one merge in the solution,
///     <c>
///         ShallowMerge&lt;Character&gt;
///     </c>
///     . This attribute covers the two cases the accessor cannot: a property declared on the merged type itself, and
///     a setter someone later widens back to protected or public. Lives here rather than beside the merge helper
///     because the properties carrying it are in AL.SocketClient, which cannot see AL.Client.
/// </remarks>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Property)]
public sealed class ShallowMergeIgnoreAttribute : Attribute;
