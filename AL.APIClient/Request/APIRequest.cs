#region
using System;
using System.Text.Json;
using AL.APIClient.Definitions;
using AL.APIClient.Json.SystemTextJson;
using AL.APIClient.Model;
using AL.Core.Helpers;
using RestSharp;
#endregion

namespace AL.APIClient.Request;

public sealed class APIRequest : RestRequest
{
    internal APIRequest(
        Method method,
        APIMethod apiMethod,
        object? arguments,
        AuthUser? authUser = null,
        string? cookieDomain = null)
        : base($"api/{EnumHelper.ToString(apiMethod)}", method)
    {
        //the api takes the argument object as a raw json body. the old method/arguments form-field
        //convention is gone and now fails dispatcher validation with "invalid_field"
        this.AddStringBody(JsonSerializer.Serialize(arguments ?? new object(), ApiJson.Options), ContentType.Json);

        if (authUser == null)
            return;

        //without a domain matching the request uri the cookie is never attached
        ArgumentNullException.ThrowIfNull(cookieDomain);

        this.AddCookie(
            "auth",
            authUser.ToString(),
            "/",
            cookieDomain);
    }
}