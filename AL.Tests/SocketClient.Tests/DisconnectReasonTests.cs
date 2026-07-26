#region
using AL.Core.Helpers;
using AL.SocketClient.Definitions;
using AL.SocketClient.SocketModel;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     Phase 8 - disconnect semantics. The server drops the connection with a
///     <c>
///         disconnect_reason
///     </c>
///     event (and, on a rate-limit kick, a
///     <c>
///         limitdcreport
///     </c>
///     ) right before the transport closes; these pin the wire names the reconnect policy branches on, and the report DTO
///     the client logs.
/// </summary>
public class DisconnectReasonTests
{
    [Test]
    public void LimitDcReportBindsTheExceptionPathMethod()
    {
        //node/server.js:4379 adds the offending method name
        const string PAYLOAD = @"{ ""calls"":{}, ""climit"":100, ""total"":50000, ""method"":""move"" }";

        var report = TestJson.Socket<LimitDcReportData>(PAYLOAD);

        report.Should()
              .NotBeNull();

        report.Method
              .Should()
              .Be("move");
    }

    [Test]
    public void LimitDcReportBindsTheServerPayload()
    {
        //node/server.js:4366 - { calls: socket.calls, climit, total: socket.total_calls }
        const string PAYLOAD = @"{ ""calls"":{ ""move"":3.5, ""attack"":1 }, ""climit"":100, ""total"":50000 }";

        var report = TestJson.Socket<LimitDcReportData>(PAYLOAD);

        report.Should()
              .NotBeNull();

        report.CallLimit
              .Should()
              .Be(100d);

        report.TotalCalls
              .Should()
              .Be(50000L);

        report.Calls
              .Should()
              .NotBeNull();

        report.Calls!.ContainsKey("move")
              .Should()
              .BeTrue();

        //method is only present on the exception-path variant (:4383)
        report.Method
              .Should()
              .BeNull();
    }

    [Test]
    [Arguments(ALSocketMessageType.DisconnectReason, "disconnect_reason")]
    [Arguments(ALSocketMessageType.LimitDcReport, "limitdcreport")]
    public void MessageTypeRendersToWireName(ALSocketMessageType messageType, string expected)
        => EnumHelper.ToString(messageType)
                     .ToLowerInvariant()
                     .Should()
                     .Be(expected);

    [Test]
    [Arguments("disconnect_reason", ALSocketMessageType.DisconnectReason)]
    [Arguments("limitdcreport", ALSocketMessageType.LimitDcReport)]
    public void WireNameParsesToMessageType(string wireName, ALSocketMessageType expected)
    {
        EnumHelper.TryParse(wireName, out ALSocketMessageType actual)
                  .Should()
                  .BeTrue($"'{wireName}' must parse");

        actual.Should()
              .Be(expected);
    }
}