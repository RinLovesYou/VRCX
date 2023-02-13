using static VrcSdk.Auth;

namespace VRCX_Server.App;

public class Auth
{
    private readonly UserSession _myUserSession;

    public Auth(UserSession userSession)
    {
        _myUserSession = userSession;
    }

    public async Task<AuthResult> Login(string username, string password)
    {
        var (authResult, message) = await _myUserSession.Api.Auth.Login(username, password);
        _myUserSession.Api.IsloggedIn = authResult == AuthResult.Success;
        UiWebSocket.SendBroadcast("Login", new
        {
            authResult = authResult.ToString(),
            message
        });
        return authResult;
    }

    public async Task<AuthResult> LoginTotp(string code)
    {
        var (authResult, message) = await _myUserSession.Api.Auth.TotpTwoFa(code);
        _myUserSession.Api.IsloggedIn = authResult == AuthResult.Success;
        UiWebSocket.SendBroadcast("Login", new
        {
            authResult = authResult.ToString(),
            message
        });
        return authResult;
    }

    public async Task<AuthResult> LoginEmail(string code)
    {
        var (authResult, message) = await _myUserSession.Api.Auth.EmailTwoFa(code);
        _myUserSession.Api.IsloggedIn = authResult == AuthResult.Success;
        UiWebSocket.SendBroadcast("Login", new
        {
            authResult = authResult.ToString(),
            message
        });
        return authResult;
    }
}