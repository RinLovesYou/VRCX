using System.Diagnostics;

namespace VrcSdk;

public class UserSession
{
    public WebRequestApi WebRequestApi { get; }
    public Config Config { get; }
    public Auth Auth { get; }
    public WebSocketApi WebSocketApi { get; }
    public CurrentUser CurrentUser { get; }

    public readonly Uri ApiUrl = new("https://api.vrchat.cloud/api/1/");
    public readonly string WebSocketUrl = "wss://pipeline.vrchat.cloud";
    public readonly string UserAgent = "VRCX/1.0";

    public string? AuthToken;

    public static List<UserSession> UserSessions = new();

    public UserSession()
    {
        WebRequestApi = new(this);
        Config = new(this);
        Auth = new(this);
        WebSocketApi = new(this);
        CurrentUser = new(this);

        UserSessions.Add(this);
    }

    ~UserSession()
    {
        UserSessions.Remove(this);
    }

    public void Logger(string message, string json = "")
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.WriteLine(message);
            return;
        }

        Debug.WriteLine($"{message} {json}");
    }
}