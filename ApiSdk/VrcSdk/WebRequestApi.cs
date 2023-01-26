using System.Net;
using System.Text;
using System.Text.Json;

namespace VrcSdk;

public class WebRequestApi
{
    private UserSession _myUserSession;
    private readonly HttpClient client;
    private CookieContainer cookies;

    public WebRequestApi(UserSession userSession)
    {
        _myUserSession = userSession;

        ServicePointManager.DefaultConnectionLimit = 10;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        cookies = new();
        LoadCookies().Wait();
        var handler = new HttpClientHandler()
        {
            CookieContainer = cookies
        };
        client = new(handler)
        {
            BaseAddress = _myUserSession.ApiUrl
        };
    }

    public class RequestData
    {
        public string url;
        public HttpMethod method;
#nullable enable
        public Dictionary<string, string>? headers;
        public string? body;
#nullable disable
    }

    public async Task<(HttpStatusCode, string)> DoRequest(RequestData requestData)
    {
        var request = new HttpRequestMessage(requestData.method, requestData.url);
        request.Headers.Add("User-Agent", _myUserSession.UserAgent);
        if (requestData.headers != null)
        {
            foreach (var header in requestData.headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }
        if (requestData.headers != null && requestData.body != null && !requestData.headers.ContainsKey("Content-Type"))
        {
            request.Headers.Add("Content-Type", "application/json; charset=utf-8");
        }
        if (requestData.body != null)
        {
            request.Content = new StringContent(requestData.body, Encoding.UTF8, "application/json");
        }
        try
        {
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            _myUserSession.Logger($"{request.RequestUri}", responseString);
            return (response.StatusCode, responseString);
        }
        catch (WebException webException)
        {
            if (webException.Response is HttpWebResponse response)
            {
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return (response.StatusCode, responseString);
            }
        }
        return (HttpStatusCode.InternalServerError, string.Empty);
    }

    public async Task SaveCookies()
    {
        await using var fs = File.OpenWrite("cookies.json");
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        JsonSerializer.Serialize(fs, cookies.GetAllCookies(), options);
    }

    public async Task LoadCookies()
    {
        if (!File.Exists("cookies.json"))
            return;
        cookies = new();
        await using var fs = File.OpenRead("cookies.json");
        var cookieCollection = JsonSerializer.Deserialize<CookieCollection>(fs);
        foreach (var cookie in cookieCollection.OfType<Cookie>())
        {
            cookie.Secure = true;
        }
        cookies.Add(cookieCollection);
    }
}