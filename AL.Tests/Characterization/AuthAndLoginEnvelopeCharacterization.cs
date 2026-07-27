#region
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json.Nodes;
using AL.APIClient;
using AL.APIClient.Model;
using AL.APIClient.Request;
using AL.APIClient.Response;
using FluentAssertions;
using RestSharp;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins the behaviour of the auth-cookie parser (<see cref="AuthUser" />) and the login/notification envelope handling
///     (
///     <c>
///         LoginResponseConverter
///     </c>
///     ,
///     <c>
///         ALAPIClient.ReadNotifications
///     </c>
///     ) across the System.Text.Json migration. Offline: every payload is a synthesized literal.
/// </summary>
/// <remarks>
///     <see cref="AuthUser" />'s constructor is
///     <c>
///         internal
///     </c>
///     and AL.APIClient does not grant AL.Tests access to its internals, and
///     <c>
///         ReadNotifications
///     </c>
///     is
///     <c>
///         private static
///     </c>
///     , so both are exercised through reflection. That is deliberate: the point is to pin the real production code, not a
///     copy of it.
/// </remarks>
public sealed class AuthAndLoginEnvelopeCharacterization
{
    #region AuthUser cookie regex
    /// <summary>
    ///     Every cookie shape the parser must split into a user id and an auth key.
    /// </summary>
    [Test]
    [Arguments("auth=abc123def-tok456ghi", "abc123def", "tok456ghi")]

    //the server strips quotes before splitting, so a wrapped value must parse identically
    [Arguments(@"auth=""qid-qtok""", "qid", "qtok")]

    //the live cookie is "US_<29 alphanumerics>-<20 alphanumerics>"; the underscore is part of the id
    [Arguments("auth=US_a83Jd0kQ29charidxxxxxxxxxxxxx-Kd82Ms91xQpZ0aBc7fRt", "US_a83Jd0kQ29charidxxxxxxxxxxxxx", "Kd82Ms91xQpZ0aBc7fRt")]

    //the token group is anchored to ';' OR end-of-string
    [Arguments("auth=nosemi_id-nosemi_tok", "nosemi_id", "nosemi_tok")]

    //RegexOptions.IgnoreCase means an "Auth=" header is accepted as readily as "auth="
    [Arguments("Auth=mixed_id-mixed_tok", "mixed_id", "mixed_tok")]

    //the token stops at the first ';', so trailing cookie attributes never bleed into AuthKey
    [Arguments("auth=attr_id-attr_tok; Max-Age=157680000; Domain=.adventure.land; Path=/; Secure", "attr_id", "attr_tok")]
    public void T15_AuthCookie_ParsesIdAndToken(string cookie, string expectedUserId, string expectedAuthKey)
    {
        var auth = CreateAuthUser(cookie);

        auth.UserID
            .Should()
            .Be(expectedUserId);

        auth.AuthKey
            .Should()
            .Be(expectedAuthKey);
    }

    [Test]
    public void T15_AuthCookie_Malformed_ThrowsWithoutLeakingCookieValue()
    {
        const string SECRET_LOOKING_TOKEN = "nodashtokenonly";
        var cookie = $"auth={SECRET_LOOKING_TOKEN}; Path=/";

        var thrown = FluentActions.Invoking(() => CreateAuthUser(cookie))
                                  .Should()
                                  .ThrowExactly<InvalidOperationException>()
                                  .Which;

        //the cookie is a live credential; the message must reveal nothing but the length
        thrown.Message
              .Should()
              .NotContain(SECRET_LOOKING_TOKEN);

        thrown.Message
              .Should()
              .Contain("length");
    }

    [Test]
    public void T15_AuthCookie_Expires_ParsedAsInvariantRfc1123()
    {
        var auth = CreateAuthUser(
            "auth=exp_id-exp_tok; Max-Age=157680000; Domain=.adventure.land; Path=/; Expires=Mon, 20 Jul 2026 12:00:00 GMT");

        auth.Expires
            .Should()
            .Be(
                new DateTime(
                    2026,
                    7,
                    20,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc));

        auth.Expires
            .Kind
            .Should()
            .Be(DateTimeKind.Utc);
    }

    [Test]
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
                .Be(
                    new DateTime(
                        2026,
                        7,
                        20,
                        12,
                        0,
                        0,
                        DateTimeKind.Utc));
        } finally
        {
            Thread.CurrentThread.CurrentCulture = previousCulture;
        }
    }
    #endregion

    #region LoginResponseConverter envelope shapes
    /// <summary>
    ///     The envelope
    ///     <c>
    ///         LoginResponseConverter
    ///     </c>
    ///     binds <paramref name="body" /> to, through the production
    ///     <c>
    ///         ApiJson.Options
    ///     </c>
    ///     the REST client deserializes with. <see cref="LoginResponse" /> is a record of five scalars and every test below
    ///     asserts all five, so the shape is pinned member by member.
    /// </summary>
    private static LoginResponse Parsed(string body) => TestJson.Api<LoginResponse>(body)!;

    [Test]
    public void T15_LoginResponse_BareArray_BindsNotifications()
    {
        const string BODY = """[{"type":"message","message":"Logged In!"},{"type":"content","html":"<div>selection</div>"}]""";

        var response = Parsed(BODY);

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

    [Test]
    public void T15_LoginResponse_FailedFalseWithInfs_BindsNotifications()
    {
        const string BODY
            = """{"failed":false,"infs":[{"type":"message","message":"Logged In!"},{"type":"content","html":"<div>selection</div>"}]}""";

        //the wrapped form must bind identically to the bare array above - the envelope is searched first, then
        //each inf in order, so message/type come from the first notification and html from the second
        var response = Parsed(BODY);

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

    [Test]
    public void T15_LoginResponse_FailedTrue_SurfacesReason()
    {
        const string BODY = """{"failed":true,"reason":"invalid_field"}""";

        var response = Parsed(BODY);

        response.Failed
                .Should()
                .BeTrue();

        response.Reason
                .Should()
                .Be("invalid_field");

        response.Message
                .Should()
                .BeNull();

        //a failure envelope carries no notification, so nothing leaks into the notification-derived members
        response.Type
                .Should()
                .BeNull();

        response.Html
                .Should()
                .BeNull();
    }
    #endregion

    #region ALAPIClient.ReadNotifications envelope shapes
    [Test]
    public void T15_ReadNotifications_BareArray_ReturnsNotifications()
    {
        const string BODY = """[{"type":"message","message":"Logged In!"},{"type":"content","html":"<div>selection</div>"}]""";

        var notifications = InvokeReadNotifications(BODY);

        notifications.Count
                     .Should()
                     .Be(2);

        notifications[0]!["message"]!.GetValue<string>()
                                     .Should()
                                     .Be("Logged In!");
    }

    [Test]
    public void T15_ReadNotifications_FailedFalseWithInfs_ReturnsInfs()
    {
        const string BODY = """{"failed":false,"infs":[{"type":"servers_and_characters","mail":0}]}""";

        var notifications = InvokeReadNotifications(BODY);

        notifications.Count
                     .Should()
                     .Be(1);

        notifications[0]!["type"]!.GetValue<string>()
                                  .Should()
                                  .Be("servers_and_characters");
    }

    [Test]
    public void T15_ReadNotifications_FailedTrue_ThrowsSurfacingReason()
    {
        const string BODY = """{"failed":true,"reason":"invalid_field"}""";

        var thrown = FluentActions.Invoking(() => InvokeReadNotifications(BODY))
                                  .Should()
                                  .ThrowExactly<InvalidOperationException>()
                                  .Which;

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
                              null,
                              [
                                  typeof(LoginInfo),
                                  typeof(string)
                              ],
                              null)
                          ?? throw new InvalidOperationException("AuthUser internal constructor not found.");

        try
        {
            return (AuthUser)constructor.Invoke(
                [
                    null,
                    cookie
                ]);
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

        var method = typeof(AlApiClient).GetMethod("ReadNotifications", BindingFlags.Static | BindingFlags.NonPublic)
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