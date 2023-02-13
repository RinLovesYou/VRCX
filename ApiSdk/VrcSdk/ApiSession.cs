using System.Diagnostics;

namespace VrcSdk;

public partial class ApiSession
{
	public WebRequestApi WebRequestApi { get; }
	public Auth Auth { get; }
	public WebSocketApi WebSocketApi { get; }

	public readonly Uri ApiUrl = new("https://api.vrchat.cloud/api/1/");
	public readonly string WebSocketUrl = "wss://pipeline.vrchat.cloud";
	public readonly string UserAgent = "VRCX/1.0";

	public bool IsloggedIn;
	public string? AuthToken;

	public static List<ApiSession> UserSessions = new();

	public ApiSession()
	{
		WebRequestApi = new(this);
		Auth = new(this);
		WebSocketApi = new(this);

		UserSessions.Add(this);
	}

	~ApiSession()
	{
		WebSocketApi.Disconnect();
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