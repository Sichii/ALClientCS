#region
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json.Nodes;
using System.Threading;
using AL.APIClient;
using AL.APIClient.Model;
using AL.APIClient.Request;
using AL.APIClient.Response;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestSharp;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins the current Newtonsoft-era behaviour of the auth-cookie parser (<see cref="AuthUser" />) and the
///     login/notification envelope handling (<c>LoginResponseConverter</c>, <c>ALAPIClient.ReadNotifications</c>)
///     so the System.Text.Json migration has a fixed target. Offline: every payload is a synthesized literal.
/// </summary>
/// <remarks>
///     <see cref="AuthUser" />'s constructor is <c>internal</c> and AL.APIClient does not grant AL.Tests access to
///     its internals, and <c>ReadNotifications</c> is <c>private static</c>, so both are exercised through
///     reflection. That is deliberate: the point is to pin the real production code, not a copy of it.
/// </remarks>
[TestClass]
public sealed class AuthAndLoginEnvelopeCharacterization
{
    #region AuthUser cookie regex

    [TestMethod]
    public void T15_AuthCookie_Plain_ParsesIdAndToken()
    {
        var auth = CreateAuthUser("auth=abc123def-tok456ghi");

        auth.UserID
            .Should()
            .Be("abc123def");

        auth.AuthKey
            .Should()
            .Be("tok456ghi");
    }

    [TestMethod]
    public void T15_AuthCookie_DoubleQuoted_StripsQuotesThenParses()
    {
        //the server strips quotes before splitting, so a wrapped value must parse identically
        var auth = CreateAuthUser("auth=\"qid-qtok\"");

        auth.UserID
            .Should()
            .Be("qid");

        auth.AuthKey
            .Should()
            .Be("qtok");
    }

    [TestMethod]
    public void T15_AuthCookie_UsPrefixedId_KeepsPrefixInUserId()
    {
        //the live cookie is "US_<29 alphanumerics>-<20 alphanumerics>"; the underscore is part of the id
        var auth = CreateAuthUser("auth=US_a83Jd0kQ29charidxxxxxxxxxxxxx-Kd82Ms91xQpZ0aBc7fRt");

        auth.UserID
            .Should()
            .Be("US_a83Jd0kQ29charidxxxxxxxxxxxxx");

        auth.AuthKey
            .Should()
            .Be("Kd82Ms91xQpZ0aBc7fRt");
    }

    [TestMethod]
    public void T15_AuthCookie_NoTrailingSemicolon_ParsesToEnd()
    {
        //the token group is anchored to ';' OR end-of-string
        var auth = CreateAuthUser("auth=nosemi_id-nosemi_tok");

        auth.UserID
            .Should()
            .Be("nosemi_id");

        auth.AuthKey
            .Should()
            .Be("nosemi_tok");
    }

    [TestMethod]
    public void T15_AuthCookie_MixedCasePrefix_ParsesCaseInsensitively()
    {
        //RegexOptions.IgnoreCase means a "Auth=" header is accepted as readily as "auth="
        var auth = CreateAuthUser("Auth=mixed_id-mixed_tok");

        auth.UserID
            .Should()
            .Be("mixed_id");

        auth.AuthKey
            .Should()
            .Be("mixed_tok");
    }

    [TestMethod]
    public void T15_AuthCookie_ExtraAttributes_StopAtFirstSemicolon()
    {
        //the token stops at the first ';', so trailing cookie attributes never bleed into AuthKey
        var auth = CreateAuthUser("auth=attr_id-attr_tok; Max-Age=157680000; Domain=.adventure.land; Path=/; Secure");

        auth.UserID
            .Should()
            .Be("attr_id");

        auth.AuthKey
            .Should()
            .Be("attr_tok");
    }

    [TestMethod]
    public void T15_AuthCookie_Malformed_ThrowsWithoutLeakingCookieValue()
    {
        const string SECRET_LOOKING_TOKEN = "nodashtokenonly";
        var cookie = $"auth={SECRET_LOOKING_TOKEN}; Path=/";

        var thrown = Assert.ThrowsException<InvalidOperationException>(() => CreateAuthUser(cookie));

        //the cookie is a live credential; the message must reveal nothing but the length
        thrown.Message
              .Should()
              .NotContain(SECRET_LOOKING_TOKEN);

        thrown.Message
              .Should()
              .Contain("length");
    }

    [TestMethod]
    public void T15_AuthCookie_Expires_ParsedAsInvariantRfc1123()
    {
        var auth = CreateAuthUser(
            "auth=exp_id-exp_tok; Max-Age=157680000; Domain=.adventure.land; Path=/; Expires=Mon, 20 Jul 2026 12:00:00 GMT");

        auth.Expires
            .Should()
            .Be(new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc));

        auth.Expires.Kind
            .Should()
            .Be(DateTimeKind.Utc);
    }

    [TestMethod]
    public void T15_AuthCookie_Expires_DoesNotShiftUnderDeDe()
    {
        var previousCulture = Thread.CurrentThread.CurrentCulture;

        try
        {
            //de-DE abbreviates Monday as "Mo", so a current-culture parse of "Mon" would fail and leave Expires default
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");

            var auth = CreateAuthUser(
                "auth=exp_id-exp_tok; Max-Age=157680000; Domain=.adventure.land; Path=/; Expires=Mon, 20 Jul 2026 12:00:00 GMT");

            auth.Expires
                .Should()
                .Be(new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc));
        } finally
        {
            Thread.CurrentThread.CurrentCulture = previousCulture;
        }
    }

    #endregion

    #region LoginResponseConverter envelope shapes

    /// <summary>
    ///     The frozen Newtonsoft envelope for <paramref name="body" />, after pinning the System.Text.Json
    ///     converter against it. <see cref="LoginResponse" /> is a record of five scalars, so a single value
    ///     comparison covers every member and the per-field assertions below stay a readable record of what the
    ///     oracle actually produces.
    /// </summary>
    private static LoginResponse ParsedByBothEngines(string body)
    {
        var response = JsonConvert.DeserializeObject<LoginResponse>(body)!;

        TestJson.Api<LoginResponse>(body)
                .Should()
                .Be(response);

        return response;
    }

    [TestMethod]
    public void T15_LoginResponse_BareArray_BindsNotifications()
    {
        const string BODY =
            """[{"type":"message","message":"Logged In!"},{"type":"content","html":"<div>selection</div>"}]""";

        var response = ParsedByBothEngines(BODY);

        response.Failed
                .Should()
                .BeFalse();

        response.Reason
                .Should()
                .BeNull();

        response.Message
                .Should()
                .Be("Logged In!");

        response.Type
                .Should()
                .Be("message");

        response.Html
                .Should()
                .Be("<div>selection</div>");
    }

    [TestMethod]
    public void T15_LoginResponse_FailedFalseWithInfs_BindsNotifications()
    {
        const string BODY =
            """{"failed":false,"infs":[{"type":"message","message":"Logged In!"},{"type":"content","html":"<div>selection</div>"}]}""";

        var response = ParsedByBothEngines(BODY);

        response.Failed
                .Should()
                .BeFalse();

        response.Reason
                .Should()
                .BeNull();

        response.Message
                .Should()
                .Be("Logged In!");

        response.Type
                .Should()
                .Be("message");

        response.Html
                .Should()
                .Be("<div>selection</div>");
    }

    [TestMethod]
    public void T15_LoginResponse_FailedTrue_SurfacesReason()
    {
        const string BODY = """{"failed":true,"reason":"invalid_field"}""";

        var response = ParsedByBothEngines(BODY);

        response.Failed
                .Should()
                .BeTrue();

        response.Reason
                .Should()
                .Be("invalid_field");

        response.Message
                .Should()
                .BeNull();
    }

    #endregion

    #region ALAPIClient.ReadNotifications envelope shapes

    [TestMethod]
    public void T15_ReadNotifications_BareArray_ReturnsNotifications()
    {
        const string BODY =
            """[{"type":"message","message":"Logged In!"},{"type":"content","html":"<div>selection</div>"}]""";

        var notifications = InvokeReadNotifications(BODY);

        notifications.Count
                     .Should()
                     .Be(2);

        notifications[0]!["message"]!
            .GetValue<string>()
            .Should()
            .Be("Logged In!");
    }

    [TestMethod]
    public void T15_ReadNotifications_FailedFalseWithInfs_ReturnsInfs()
    {
        const string BODY =
            """{"failed":false,"infs":[{"type":"servers_and_characters","mail":0}]}""";

        var notifications = InvokeReadNotifications(BODY);

        notifications.Count
                     .Should()
                     .Be(1);

        notifications[0]!["type"]!
            .GetValue<string>()
            .Should()
            .Be("servers_and_characters");
    }

    [TestMethod]
    public void T15_ReadNotifications_FailedTrue_ThrowsSurfacingReason()
    {
        const string BODY = """{"failed":true,"reason":"invalid_field"}""";

        var thrown = Assert.ThrowsException<InvalidOperationException>(() => InvokeReadNotifications(BODY));

        thrown.Message
              .Should()
              .Contain("invalid_field");
    }

    #endregion

    #region Reflection helpers

    private static AuthUser CreateAuthUser(string cookie)
    {
        var constructor = typeof(AuthUser).GetConstructor(
                              BindingFlags.Instance | BindingFlags.NonPublic,
                              binder: null,
                              types: [typeof(LoginInfo), typeof(string)],
                              modifiers: null)
                          ?? throw new InvalidOperationException("AuthUser internal constructor not found.");

        try
        {
            return (AuthUser)constructor.Invoke([null, cookie]);
        } catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            //rethrow the real exception so tests assert against it rather than the reflection wrapper
            ExceptionDispatchInfo.Capture(ex.InnerException)
                                 .Throw();

            throw;
        }
    }

    private static JsonArray InvokeReadNotifications(string content)
    {
        var response = new RestResponse(new RestRequest())
        {
            Content = content,
            IsSuccessStatusCode = true,
            ResponseStatus = ResponseStatus.Completed,
            StatusCode = HttpStatusCode.OK
        };

        var method = typeof(ALAPIClient).GetMethod("ReadNotifications", BindingFlags.Static | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("ReadNotifications method not found.");

        try
        {
            return (JsonArray)method.Invoke(null, [response])!;
        } catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException)
                                 .Throw();

            throw;
        }
    }

    #endregion
}
