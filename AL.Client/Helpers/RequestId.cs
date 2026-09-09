#region
using System;
#endregion

namespace AL.Client.Helpers;

/// <summary>
///     Mints the opaque token an emit sends as <c>request_id</c> so the server can echo it back on the reply.
/// </summary>
/// <remarks>
///     The server neither parses nor bounds the value - fail_response and success_response copy it into the reply
///     object as it arrived. Uniqueness per in-flight call is the only requirement, so a GUID's digits are enough and
///     nothing here needs to be guessable or ordered.
/// </remarks>
internal static class RequestId
{
    public static string New()
        => Guid.NewGuid()
               .ToString("N");
}
