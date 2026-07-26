#region
using System;
#endregion

namespace AL.Core.Json.Attributes;

/// <summary>
///     Marks a type that serializes as a named object even though it implements
///     <see cref="System.Collections.IEnumerable" />, so <see cref="Json.SystemTextJson.ForcedObjectConverterFactory" />
///     claims it before System.Text.Json can classify it as a collection. May be applied to an interface, in which case
///     every implementer inherits the contract - <see cref="AL.Core.Interfaces.IRectangle" /> carries it for the geometry
///     containers that enumerate their vertices for bounding-box convenience.
/// </summary>
/// <remarks>
///     Replaces Newtonsoft's
///     <c>
///         [JsonObject]
///     </c>
///     , which served this purpose while both engines ran side by side. System.Text.Json has no attribute that overrides
///     an object/collection classification, so the marker has to be local.
/// </remarks>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public sealed class JsonForcedObjectAttribute : Attribute;