namespace VirtualQueueApi.Domain.Models.Commands.AuthCommands;

public class ResendCodeCommand
{
    public string Token { get; set; } = string.Empty;        
}
