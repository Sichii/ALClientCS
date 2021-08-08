using AL.APIClient.Definitions;
using AL.APIClient.Model;
using AL.Core.Helpers;
using Newtonsoft.Json;
using RestSharp;

namespace AL.APIClient.Request
{
    internal class APIRequest : RestRequest
    {
        internal APIRequest(Method method, APIMethod apiMethod, object? arguments, AuthUser? authUser = null)
            : base($"api/{EnumHelper.ToString(apiMethod)}", method)
        {
            //AddHeader("Content-Type", "application/x-www-form-urlencoded");
            AddParameter("method", EnumHelper.ToString(apiMethod));

            if (arguments != null)
                AddParameter("arguments", JsonConvert.SerializeObject(arguments));

            if (authUser != null)
                AddCookie("auth", authUser.ToString());
        }
    }
}