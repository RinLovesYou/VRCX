using Newtonsoft.Json;
using System.Security.Authentication;
using WebSocketSharp;

namespace VrcSdk;

public partial class WebSocketApi
{
	public WebSocket webSocket;
	private EventHandler<CloseEventArgs> onCloseHandler;
	private readonly ApiSession _myApiSession;

	public WebSocketApi(ApiSession userSession)
	{
		_myApiSession = userSession;
	}

	public void Disconnect()
	{
		webSocket.OnClose -= onCloseHandler;
		webSocket?.Close();
	}

	public void Connect()
	{
		if (string.IsNullOrEmpty(_myApiSession.AuthToken))
		{
			return;
		}
		webSocket = new WebSocket($"{_myApiSession.WebSocketUrl}/?auth={_myApiSession.AuthToken}");
		// Debug for mitmproxy
		webSocket.SslConfiguration.EnabledSslProtocols = SslProtocols.Default | SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
		webSocket.CustomHeaders = new Dictionary<string, string>
			{
				{"User-Agent", _myApiSession.UserAgent}
			};
		webSocket.OnMessage += (sender, e) =>
		{
			_myApiSession.Logger($"WebSocket", e.Data);
			var message = JsonConvert.DeserializeObject<dynamic>(e.Data);
			// ParseResponse(message);
		};
		onCloseHandler = (sender, e) =>
		{
			switch (e.Code)
			{
				case 1001 or 1005 or 1006:
					// timeout
					Thread.Sleep(3000);
					_myApiSession.Logger($"Disconnected ({e.Code}), Attempting to reconnect...");
					Connect();
					break;

				default:
					// unknown error
					_myApiSession.Logger($"Disconnected: {e.Code} - {e.Reason}");
					Thread.Sleep(3000);
					_myApiSession.Logger($"Attempting to reconnect...");
					Connect();
					break;
			}
		};
		webSocket.OnClose += onCloseHandler;
		webSocket.OnOpen += (sender, e) =>
		{
			_myApiSession.Logger($"Connected");
		};
		webSocket.OnError += (sender, e) =>
		{
			_myApiSession.Logger($"Error: " + e.Message + e.Exception);
			if (webSocket.IsAlive)
				webSocket.Close();
		};

		webSocket.Connect();
	}
}