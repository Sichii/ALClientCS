#region
#endregion

namespace AL.Client.Helpers;

/// <summary>
///     Mints the opaque token an emit sends as <c>request_id</c> so the server can echo it back on the reply.
/// </summary>
/// <remarks>
///     The server copies the value into its reply unparsed, so it only has to be unique per in-flight call.
/// </remarks>
internal static class RequestId
{
    public static string Create()
        => Guid.NewGuid()
               .ToString("N");
}