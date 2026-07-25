#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using AL.APIClient.Request;
using AL.Client;
using AL.Core.Abstractions;
using AL.Core.Json;
using AL.Core.Json.SystemTextJson;
using AL.Data;
using AL.Pathfinding;
using AL.SocketClient.SocketModel;
using AL.Tests.Characterization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace AL.Tests.SystemTextJson.Tests;

/// <summary>
///     Phase 6a's instrument. Resolves every type in the six library assemblies twice — once through
///     <see cref="NewtonsoftAttributeBindingModifier" />, once through a bare resolver — and pins the exact
///     difference between the two contracts.
/// </summary>
/// <remarks>
///     Phase 6a replaces the modifier with native System.Text.Json attributes. Every attribute it adds is a
///     <b>no-op while the modifier is still installed</b>, because the modifier already produced that binding
///     from the Newtonsoft attribute — so a missing or wrong native attribute stays completely masked right up
///     until the flip that removes the modifier, at which point the member silently binds to its default.
///     Nothing else in the suite can see through that mask: the five fixture differentials between them reach a
///     few hundred members, against ~1,800 here.
///     <para>
///         So the delta is the work list. It starts at whatever the modifier currently adds, and every batch of
///         native attributes shrinks it. The flip is safe exactly when it reaches zero, and the fixture is
///         re-baselined <b>downward only</b> — a row appearing is always a regression.
///     </para>
///     <para>Deleted in Phase 6b along with the modifier it measures.</para>
/// </remarks>
[TestClass]
public sealed class BindingParityCharacterization
{
    private const string DELTA_SNAPSHOT = "binding-parity-delta.txt";

    private const string GENERATED_DELTA_SNAPSHOT = "binding-parity-delta.generated.txt";

    /// <summary>
    ///     The same six assemblies the T11 census walks, resolved through one representative type each so a
    ///     rename fails to compile rather than silently narrowing the survey.
    /// </summary>
    private static readonly IReadOnlyList<Assembly> LibraryAssemblies = new[]
    {
        typeof(AttributedObjectBase).Assembly, // AL.Core
        typeof(GameData).Assembly,             // AL.Data
        typeof(ALClient).Assembly,             // AL.Client
        typeof(StartData).Assembly,            // AL.SocketClient
        typeof(LoginInfo).Assembly,            // AL.APIClient
        typeof(Pathfinder).Assembly            // AL.Pathfinding
    }.Distinct()
     .ToArray();

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void P6_BindingParity_DeltaMatchesCommittedFixture()
    {
        var delta = BuildDelta();
        var rendered = string.Join("\n", delta);

        var generatedPath = Fixture.WriteSnapshot(GENERATED_DELTA_SNAPSHOT, rendered);
        var committed = Fixture.ReadCommittedSnapshot(DELTA_SNAPSHOT);

        TestContext.WriteLine($"binding-parity delta rows: {delta.Count}");

        committed.Should()
                 .NotBeNull(
                     $"the delta fixture must be committed at AL.Tests/Fixtures/snapshots/{DELTA_SNAPSHOT}; "
                     + $"copy the generated file from {generatedPath} into the repo and re-run");

        Normalize(committed!)
            .Should()
            .Be(
                Normalize(rendered),
                "the modifier-vs-native binding delta moved. Phase 6a shrinks this fixture as native attributes "
                + "land, so a row DISAPPEARING is progress and the fixture is re-baselined; a row APPEARING means "
                + "a native attribute was lost or a member changed shape, and is a regression");
    }

    /// <summary>
    ///     Every row the two resolvers disagree on, sorted for a stable fixture. <c>-</c> is a binding only the
    ///     modifier produces (the Phase 6a work list); <c>+</c> is one only native resolution produces.
    /// </summary>
    private static List<string> BuildDelta()
    {
        var withModifier = BuildOptions(applyModifier: true);
        var native = BuildOptions(applyModifier: false);
        var rows = new List<string>();

        foreach (var type in SurveyedTypes())
        {
            var modifierContract = Describe(withModifier, type);
            var nativeContract = Describe(native, type);

            foreach (var row in modifierContract.Except(nativeContract, StringComparer.Ordinal))
                rows.Add($"-\t{type.FullName}\t{row}");

            foreach (var row in nativeContract.Except(modifierContract, StringComparer.Ordinal))
                rows.Add($"+\t{type.FullName}\t{row}");
        }

        rows.Sort(StringComparer.Ordinal);

        return rows;
    }

    /// <summary>
    ///     A copy of the production options differing only in whether the binding modifier is installed, so the
    ///     delta isolates the modifier and nothing else. The converter list is identical on both sides, which
    ///     matters: a converter-claimed type has no properties at all, and that must not read as a difference.
    /// </summary>
    private static JsonSerializerOptions BuildOptions(bool applyModifier)
    {
        var resolver = new DefaultJsonTypeInfoResolver();

        if (applyModifier)
            resolver.Modifiers.Add(NewtonsoftAttributeBindingModifier.Modify);

        return new JsonSerializerOptions(ALJson.Options)
        {
            TypeInfoResolver = resolver
        };
    }

    /// <summary>
    ///     One line per bound member, plus a single line for a type the resolver hands to a converter or refuses
    ///     outright. A refusal is recorded rather than skipped: an unresolved name collision throws
    ///     <see cref="InvalidOperationException" /> here, which is exactly the signal Phase 6a needs to see.
    /// </summary>
    private static IReadOnlyList<string> Describe(JsonSerializerOptions options, Type type)
    {
        JsonTypeInfo typeInfo;

        try
        {
            typeInfo = options.GetTypeInfo(type);
        } catch (Exception ex)
        {
            return [$"<throws>\t{ex.GetType().Name}\t{FirstLine(ex.Message)}"];
        }

        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return [$"<kind>\t{typeInfo.Kind}\t{typeInfo.Converter.GetType().Name}"];

        return typeInfo.Properties
                       .Select(
                           property => string.Join(
                               '\t',
                               property.Name,
                               FriendlyName(property.PropertyType),
                               property.Get is null ? "get=no" : "get=yes",
                               property.Set is null ? "set=no" : "set=yes",
                               property.CustomConverter is null ? "conv=none" : $"conv={property.CustomConverter.GetType().Name}"))
                       .OrderBy(row => row, StringComparer.Ordinal)
                       .ToArray();
    }

    /// <summary>
    ///     Concrete, closed, non-enum types the resolver could produce an object contract for. Open generics and
    ///     compiler-generated types are excluded: neither is ever a wire shape and both add churn to the fixture.
    /// </summary>
    private static IEnumerable<Type> SurveyedTypes()
        => LibraryAssemblies.SelectMany(GetTypes)
                            .Where(
                                type => !type.IsGenericTypeDefinition
                                        && !type.IsEnum
                                        && !type.IsInterface
                                        && !type.IsAbstract
                                        && !type.IsPointer
                                        && !type.IsByRef
                                        && (type.GetCustomAttribute<CompilerGeneratedAttribute>() is null)
                                        && !type.Name.Contains('<', StringComparison.Ordinal))
                            .OrderBy(type => type.FullName, StringComparer.Ordinal);

    //a type that fails to load its dependencies must narrow the survey visibly, not abort the whole run
    private static IEnumerable<Type> GetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        } catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type is not null)!;
        }
    }

    private static string FriendlyName(Type type)
        => type.IsGenericType
            ? $"{type.Name[..type.Name.IndexOf('`', StringComparison.Ordinal)]}<{string.Join(',', type.GetGenericArguments().Select(FriendlyName))}>"
            : type.Name;

    private static string FirstLine(string message) => message.Split('\n')[0].Trim();

    private static string Normalize(string text) => text.Replace("\r\n", "\n");
}
