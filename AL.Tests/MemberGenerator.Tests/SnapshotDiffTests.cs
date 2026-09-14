#region
using System.Text.Json.Nodes;
using AL.MemberGenerator;
using FluentAssertions;
#endregion

namespace AL.Tests.MemberGenerator.Tests;

/// <summary>
///     The value diff a refresh reports: what moved behind the keys that stayed, in the lines the report carries. Restated
///     here rather than read back off a fetch, so a diff that folded the wrong section or lost a leaf cannot agree with
///     itself.
/// </summary>
public class SnapshotDiffTests
{
    [Test]
    public void MovedValuesAddedAndRemovedEntriesAreReportedPerSection()
    {
        var old = JsonNode.Parse(
                              """
                              {"version":1,
                               "items":{"firestaff":{"range":60,"g":15000,"stat":[1,2]},"oldsword":{"g":1}},
                               "levels":[10,20],
                               "sprites":{"a":{"x":1}}}
                              """)!
                          .AsObject();

        var fresh = JsonNode.Parse(
                                """
                                {"version":2,
                                 "items":{"firestaff":{"range":70,"g":15000,"stat":[1,3],"dmg":5},"newsword":{"g":2}},
                                 "levels":[10,25],
                                 "sprites":{"a":{"x":2}}}
                                """)!
                            .AsObject();

        //a list section is one entry named after itself; a presentation section keeps its count and loses its lines
        Snapshots.Diff(old, fresh)
                 .Should()
                 .Equal(
                     "== items: 1 changed, 1 added, 1 removed",
                     "  + newsword",
                     "  - oldsword",
                     "  firestaff.dmg: (new) 5",
                     "  firestaff.range: 60 -> 70",
                     "  firestaff.stat[1]: 2 -> 3",
                     "== levels: 1 changed",
                     "  levels[1]: 20 -> 25",
                     "== sprites: 1 changed",
                     "",
                     "summary: items 1 changed, 1 added, 1 removed; levels 1 changed; sprites 1 changed");
    }

    [Test]
    public void OnlyTheVersionMovingReportsNoChanges()
    {
        var old = JsonNode.Parse("""{"version":1,"items":{"a":{"g":1,"stat":[1,2]}}}""")!.AsObject();
        var fresh = JsonNode.Parse("""{"version":2,"items":{"a":{"g":1,"stat":[1,2]}}}""")!.AsObject();

        Snapshots.Diff(old, fresh)
                 .Should()
                 .Equal("", "summary: no value changes");
    }
}
