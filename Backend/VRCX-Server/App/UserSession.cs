using System.Diagnostics;
using VrcSdk;

namespace VRCX_Server.App;

public class UserSession
{
    public static List<UserSession> UserSessions = new();

    public UserSession()
    {
        Api = new ApiSession();
        Auth = new Auth(this);

        UserSessions.Add(this);
    }

    public ApiSession Api { get; }
    public Auth Auth { get; }

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