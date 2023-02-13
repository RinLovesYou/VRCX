using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using VRCX_Server.App;
using VRCX_Server.Controllers;

namespace VRCX_Server;

internal class JsonType
{
    public string type { get; set; }
}

internal class LoginMessage
{
    public string type { get; set; }
    public string username { get; set; }
    public string password { get; set; }
    public string code { get; set; }
}

public class UiWebSocket
{
    private const int BufferSize = 1024 * 4;
    private readonly UserSession _myUserSession;

    public UiWebSocket(UserSession userSession)
    {
        _myUserSession = userSession;
    }

    public void OnMessage(byte[] buffer)
    {
        var size = Array.IndexOf(buffer, (byte)0);
        var json = Encoding.UTF8.GetString(buffer, 0, size < 0 ? BufferSize : size);
        var jsonType = JsonConvert.DeserializeObject<JsonType>(json);
        switch (jsonType.type)
        {
            case "Login":
                var loginMessage = JsonConvert.DeserializeObject<LoginMessage>(json);
                _myUserSession.Auth.Login(loginMessage.username, loginMessage.password);
                break;
            case "LoginTotp":
                var totpLoginMessage = JsonConvert.DeserializeObject<LoginMessage>(json);
                _myUserSession.Auth.LoginTotp(totpLoginMessage.code);
                break;
            case "LoginEmail":
                var emailLoginMessage = JsonConvert.DeserializeObject<LoginMessage>(json);
                _myUserSession.Auth.LoginEmail(emailLoginMessage.code);
                break;
            default:
                Debug.WriteLine("Unknown message type: " + json);
                break;
        }
    }

    public static void SendBroadcast(string type, object message)
    {
        var json = JsonConvert.SerializeObject(new { type, message });
        var serverMsg = Encoding.UTF8.GetBytes(json);
        foreach (var connection in WebSocketsController.Connections.Keys)
        {
            connection.SendAsync(serverMsg, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}