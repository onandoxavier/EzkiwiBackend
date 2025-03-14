namespace VirtualQueueApi.Domain.Models.Commands.AuthCommands;

public class ForgotPasswordCommand
{
    public string Email { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}
