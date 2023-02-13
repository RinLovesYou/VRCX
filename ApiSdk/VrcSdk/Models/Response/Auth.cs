namespace VrcSdk.Response;

public class Auth
{
    public bool ok { get; set; }
    public string token { get; set; }
}

public class Error
{
    public ErrorMessage error { get; set; }
}

public class ErrorMessage
{
    public string message { get; set; }
    public int status_code { get; set; }
}