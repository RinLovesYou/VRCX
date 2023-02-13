using System.Net;
using System.Text;
using Newtonsoft.Json;
using VrcSdk.Response;
using TwoFa = VrcSdk.Request.TwoFa;

namespace VrcSdk;

public class Auth
{
    public enum AuthResult
    {
        Error,
        Success,
        TotpRequired,
        EmailRequired
    }

    private readonly ApiSession _myApiSession;

    public Auth(ApiSession userSession)
    {
        _myApiSession = userSession;
    }

    public async Task<(AuthResult, string)> Login(string username, string password)
    {
        var usernameEncoded = WebUtility.UrlEncode(username);
        var passwordEncoded = WebUtility.UrlEncode(password);
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{usernameEncoded}:{passwordEncoded}"));
        var (status, responseJson) = await _myApiSession.WebRequestApi.DoRequest(new WebRequestApi.RequestData
        {
            url = "auth/user",
            method = HttpMethod.Get,
            headers = new Dictionary<string, string>
            {
                { "Authorization", $"Basic {auth}" }
            }
        });
        if (status != HttpStatusCode.OK)
        {
            _myApiSession.Logger("Login error", responseJson);
            var errorMessage = responseJson;
            try
            {
                var error = JsonConvert.DeserializeObject<Error>(responseJson);
                errorMessage = error.error.message;
            }
            catch (Exception)
            {
                // ignored
            }

            return (AuthResult.Error, errorMessage);
        }

        var authResponse = JsonConvert.DeserializeObject<dynamic>(responseJson);
        if (authResponse?.requiresTwoFactorAuth != null && !authResponse?.requiresTwoFactorAuth.HasValues)
        {
            // no 2FA required
            return (AuthResult.Success, string.Empty);
        }

        if (authResponse?.requiresTwoFactorAuth.First == "emailOtp")
        {
            return (AuthResult.EmailRequired, string.Empty);
        }

        return (AuthResult.TotpRequired, string.Empty);
    }

    public async Task<Response.Auth> GetAuth()
    {
        var (status, responseJson) = await _myApiSession.WebRequestApi.DoRequest(new WebRequestApi.RequestData
        {
            url = "auth",
            method = HttpMethod.Get
        });
        if (status == HttpStatusCode.OK)
        {
            var auth = JsonConvert.DeserializeObject<Response.Auth>(responseJson);
            if (auth.ok)
            {
                _myApiSession.AuthToken = auth.token;
                _ = _myApiSession.WebRequestApi.SaveCookies();
                return auth;
            }
        }

        throw new Exception($"Failed to get auth: {status} {responseJson}");
    }

    public async Task<(AuthResult, string)> EmailTwoFa(string code)
    {
        var (status, responseJson) = await _myApiSession.WebRequestApi.DoRequest(new WebRequestApi.RequestData
        {
            url = "auth/twofactorauth/emailotp/verify",
            method = HttpMethod.Post,
            body = JsonConvert.SerializeObject(new TwoFa
            {
                code = code
            })
        });
        if (status != HttpStatusCode.OK)
        {
            return (AuthResult.Error, "Invalid email code");
        }

        var response = JsonConvert.DeserializeObject<Response.TwoFa>(responseJson);
        if (response.verified)
        {
            return (AuthResult.Success, string.Empty);
        }

        return (AuthResult.EmailRequired, string.Empty);
    }

    public async Task<(AuthResult, string)> TotpTwoFa(string code)
    {
        var (status, responseJson) = await _myApiSession.WebRequestApi.DoRequest(new WebRequestApi.RequestData
        {
            url = "auth/twofactorauth/totp/verify",
            method = HttpMethod.Post,
            body = JsonConvert.SerializeObject(new TwoFa
            {
                code = code
            })
        });
        if (status != HttpStatusCode.OK)
        {
            return (AuthResult.Error, string.Empty);
        }

        var response = JsonConvert.DeserializeObject<Response.TwoFa>(responseJson);
        if (response.verified)
        {
            return (AuthResult.Success, "Invalid TOTP code");
        }

        return (AuthResult.TotpRequired, string.Empty);
    }
}