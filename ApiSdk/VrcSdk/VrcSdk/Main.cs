using Newtonsoft.Json;

namespace VrcSdk;

internal class Program
{
    // just for testing
    public static void Main(string[] args)
    {
        var userSession = new UserSession();

        var username = "";
        var password = "";
        userSession.Config.Fetch().Wait();
        if (!userSession.Auth.Login(username, password).Result)
        {
            Console.WriteLine("Login failed");
            Console.ReadLine();
            return;
        }
        userSession.Auth.GetAuth().Wait();
        userSession.WebSocketApi.Connect();

        Console.ReadLine();
    }
}