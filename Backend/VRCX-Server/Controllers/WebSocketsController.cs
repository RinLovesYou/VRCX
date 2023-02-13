using System.Diagnostics;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;

namespace VRCX_Server.Controllers;

[ApiController]
[Route("[controller]")]
public class WebSocketsController : ControllerBase
{
    public static Dictionary<WebSocket, UiWebSocket> Connections = new();

    [HttpGet("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            Debug.WriteLine("WebSocket connection established");
            var userSession = Program.UserSessions["0"]; // hardcode for now
            var uiWebSocket = new UiWebSocket(userSession);
            Connections.Add(webSocket, uiWebSocket);
            await Receive(webSocket, uiWebSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private async Task Receive(WebSocket webSocket, UiWebSocket uiWebSocket)
    {
        try
        {
            const int bufferSize = 1024 * 4;
            var buffer = new byte[bufferSize];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            uiWebSocket.OnMessage(buffer);
            Debug.WriteLine("Message received from Client");

            while (!result.CloseStatus.HasValue)
            {
                buffer = new byte[bufferSize];
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                uiWebSocket.OnMessage(buffer);
                Debug.WriteLine("Message received from Client");
            }

            Debug.WriteLine("websocket done");

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
        finally
        {
            Connections.Remove(webSocket);
            Debug.WriteLine("WebSocket connection closed");
        }
    }
}