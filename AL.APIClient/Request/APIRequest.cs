#region
using AL.APIClient.Definitions;
using AL.APIClient.Model;
using AL.Core.Helpers;
using Newtonsoft.Json;
using RestSharp;
#endregion

namespace AL.APIClient.Request;

public sealed class APIRequest : RestRequest
{
    internal APIRequest(
        Method method,
        APIMethod apiMethod,
        object? arguments,
        AuthUser? authUser = null)
        : base($"api/{EnumHelper.ToString(apiMethod)}", method)
    {
        //AddHeader("Content-Type", "application/x-www-form-urlencoded");
        this.AddParameter("method", EnumHelper.ToString(apiMethod));

        if (arguments != null)
            this.AddParameter("arguments", JsonConvert.SerializeObject(arguments));

        if (authUser != null)
            this.AddCookie(
                "auth",
                authUser.ToString(),
                string.Empty,
                string.Empty);
    }
}