using System.Net;
using System.Text;
using System.Text.Json;

namespace VrcSdk;

public class WebRequestApi
{
    private readonly ApiSession _myApiSession;
    private readonly HttpClient client;
    private CookieContainer cookies;

    public WebRequestApi(ApiSession userSession)
    {
        _myApiSession = userSession;

        ServicePointManager.DefaultConnectionLimit = 10;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        cookies = new CookieContainer();
        //LoadCookies().Wait();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookies
        };
        client = new HttpClient(handler)
        {
            BaseAddress = _myApiSession.ApiUrl
        };
    }

    public class RequestData
    {
        public string url;
        public HttpMethod method;
#nullable enable
        public string? body;
        public Dictionary<string, string>? headers;
#nullable disable
    }

    public async Task<(HttpStatusCode, string)> DoRequest(RequestData requestData)
    {
        var request = new HttpRequestMessage(requestData.method, requestData.url);
        request.Headers.Add("User-Agent", _myApiSession.UserAgent);
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
            _myApiSession.Logger($"{request.RequestUri}", responseString);
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
        cookies = new CookieContainer();
        await using var fs = File.OpenRead("cookies.json");
        var cookieCollection = JsonSerializer.Deserialize<CookieCollection>(fs);
        foreach (var cookie in cookieCollection.OfType<Cookie>())
        {
            cookie.Secure = true;
        }

        cookies.Add(cookieCollection);
    }
}