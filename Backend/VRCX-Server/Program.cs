using VRCX_Server.App;

namespace VRCX_Server;

public class Program
{
    public static Dictionary<string, UserSession> UserSessions = new();

    public static void Main(string[] args)
    {
        UserSessions.Add("0", new UserSession());
        WebServer.Start(args);

        // var username = "";
        // var password = "";
        // UserSession.Api.GetConfig().Wait();
        // if (!UserSession.Api.GetCurrentUser().Result)
        // {
        //     if (!UserSession.Auth.Login(username, password).Result)
        //     {
        //         Console.WriteLine("Login failed");
        //         Console.ReadLine();
        //         return;
        //     }
        // }
        //
        // UserSession.Api.Auth.GetAuth().Wait();
        // UserSession.Api.WebSocketApi.Connect();

        // public async Task<bool> Login(string username, string password)
        // {
        //     var result = await _myUserSession.Api.Auth.Login(username, password);
        //     switch (result)
        //     {
        //         case AuthResult.Error:
        //             return false;
        //
        //         case AuthResult.Success:
        //             return true;
        //
        //         case AuthResult.TotpRequired:
        //             Console.WriteLine("Enter TOTP 2FA code:");
        //             return await _myUserSession.Api.Auth.Totp2Fa(Console.ReadLine());
        //
        //         case AuthResult.EmailRequired:
        //             Console.WriteLine("Enter Email 2FA code:");
        //             return await _myUserSession.Api.Auth.Email2Fa(Console.ReadLine());
        //     }
        //     return false;
        // }
        Console.ReadLine();
    }
}