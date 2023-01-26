using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace VrcSdk;

public class CurrentUser
{
    private UserSession _myUserSession;

    public CurrentUser(UserSession userSession)
    {
        _myUserSession = userSession;
    }

    public async Task<bool> Fetch()
    {
        var (status, responseJson) = await _myUserSession.WebRequestApi.DoRequest(new WebRequestApi.RequestData
        {
            url = "auth/user",
            method = HttpMethod.Get
        });
        if (status == HttpStatusCode.OK)
        {
            return true;
        }
        return false;
    }
}