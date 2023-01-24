using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace VrcSdk;

public class Auth
{
    private UserSession _myUserSession;

    public Auth(UserSession userSession)
    {
        _myUserSession = userSession;
    }

    public async Task<bool> Login(string username, string password)
    {
        var usernameEncoded = WebUtility.UrlEncode(username);
        var passwordEncoded = WebUtility.UrlEncode(password);
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{usernameEncoded}:{passwordEncoded}"));
        var (status, responseJson) = await _myUserSession.WebRequestApi.DoRequest(new WebRequestApi.RequestData
        {
            url = "auth/user",
            method = HttpMethod.Get,
            headers = new()
            {
                { "Authorization", $"Basic {auth}" }
            }
        });
        if (status != HttpStatusCode.OK)
        {
            return false;
        }
        var authResponse = JsonConvert.DeserializeObject<dynamic>(responseJson);
        if (authResponse.requiresTwoFactorAuth != null && !authResponse.requiresTwoFactorAuth.HasValues)
        {
            // no 2FA required
            return true;
        }
        Console.WriteLine("Enter 2FA code:");
        var code = Console.ReadLine();
        if (authResponse.requiresTwoFactorAuth.First == "emailOtp")
        {
            return await Email2Fa(code);
        }
        return await Totp2Fa(code);
    }

    public async Task<Response.Auth> GetAuth()
    {
        var (status, responseJson) = await _myUserSession.WebRequestApi.DoRequest(new WebRequestApi.RequestData
        {
            url = "auth",
            method = HttpMethod.Get
        });
        if (status == HttpStatusCode.OK)
        {
            var auth = JsonConvert.DeserializeObject<Response.Auth>(responseJson);
            if (auth.ok)
            {
                _myUserSession.AuthToken = auth.token;
                return auth;
            }
        }
        throw new Exception($"Failed to get auth: {status} {responseJson}");
    }

    public async Task<bool> Email2Fa(string code)
    {
        var (status, responseJson) = await _myUserSession.WebRequestApi.DoRequest(new WebRequestApi.RequestData
        {
            url = "auth/twofactorauth/emailotp/verify",
            method = HttpMethod.Post,
            body = JsonConvert.SerializeObject(new Request.TwoFa
            {
                code = code
            })
        });
        if (status != HttpStatusCode.OK)
        {
            return false;
        }
        var response = JsonConvert.DeserializeObject<Response.TwoFa>(responseJson);
        return response.verified;
    }

    public async Task<bool> Totp2Fa(string code)
    {
        var (status, responseJson) = await _myUserSession.WebRequestApi.DoRequest(new WebRequestApi.RequestData
        {
            url = "auth/twofactorauth/totp/verify",
            method = HttpMethod.Post,
            body = JsonConvert.SerializeObject(new Request.TwoFa
            {
                code = code
            })
        });
        if (status != HttpStatusCode.OK)
        {
            return false;
        }
        var response = JsonConvert.DeserializeObject<Response.TwoFa>(responseJson);
        return response.verified;
    }
}