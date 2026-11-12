namespace AuthServer.Models;

public sealed class LogoutViewModel
{
    public string? ReturnUrl { get; set; }
    public Exception? Ex { get; set; }
}