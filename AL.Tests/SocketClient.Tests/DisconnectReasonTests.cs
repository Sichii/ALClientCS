#region
using AL.Core.Helpers;
using AL.SocketClient.Definitions;
using AL.SocketClient.SocketModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     Phase 8 - disconnect semantics. The server drops the connection with a <c>disconnect_reason</c> event
///     (and, on a rate-limit kick, a <c>limitdcreport</c>) right before the transport closes; these pin the
///     wire names the reconnect policy branches on, and the report DTO the client logs.
/// </summary>
[TestClass]
public class DisconnectReasonTests
{
   [DataTestMethod]
   [DataRow("disconnect_reason", ALSocketMessageType.DisconnectReason)]
   [DataRow("limitdcreport", ALSocketMessageType.LimitDcReport)]
   public void WireNameParsesToMessageType(string wireName, ALSocketMessageType expected)
   {
      Assert.IsTrue(EnumHelper.TryParse(wireName, out ALSocketMessageType actual), $"'{wireName}' must parse");
      Assert.AreEqual(expected, actual);
   }

   [DataTestMethod]
   [DataRow(ALSocketMessageType.DisconnectReason, "disconnect_reason")]
   [DataRow(ALSocketMessageType.LimitDcReport, "limitdcreport")]
   public void MessageTypeRendersToWireName(ALSocketMessageType messageType, string expected)
      => Assert.AreEqual(expected, EnumHelper.ToString(messageType).ToLowerInvariant());

   [TestMethod]
   public void LimitDcReportBindsTheServerPayload()
   {
      //node/server.js:4366 - { calls: socket.calls, climit, total: socket.total_calls }
      const string PAYLOAD = @"{ ""calls"":{ ""move"":3.5, ""attack"":1 }, ""climit"":100, ""total"":50000 }";

      var report = TestJson.Socket<LimitDcReportData>(PAYLOAD);

      Assert.IsNotNull(report);
      Assert.AreEqual(100d, report.CallLimit);
      Assert.AreEqual(50000L, report.TotalCalls);
      Assert.IsNotNull(report.Calls);
      Assert.IsTrue(report.Calls!.ContainsKey("move"));
      //method is only present on the exception-path variant (:4383)
      Assert.IsNull(report.Method);
   }

   [TestMethod]
   public void LimitDcReportBindsTheExceptionPathMethod()
   {
      //node/server.js:4379 adds the offending method name
      const string PAYLOAD = @"{ ""calls"":{}, ""climit"":100, ""total"":50000, ""method"":""move"" }";

      var report = TestJson.Socket<LimitDcReportData>(PAYLOAD);

      Assert.IsNotNull(report);
      Assert.AreEqual("move", report.Method);
   }
}
