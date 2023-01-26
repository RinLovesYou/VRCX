using Newtonsoft.Json;

namespace VrcSdk;

public class Config
{
    private UserSession _myUserSession;

    public Config(UserSession userSession)
    {
        _myUserSession = userSession;
    }

    public async Task<dynamic> Fetch()
    {
        var (status, responseJson) = await _myUserSession.WebRequestApi.DoRequest(new WebRequestApi.RequestData
        {
            url = "config",
            method = HttpMethod.Get
        });
        return JsonConvert.DeserializeObject(responseJson);
    }
}