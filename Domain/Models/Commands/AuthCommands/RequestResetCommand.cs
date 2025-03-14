namespace VirtualQueueApi.Domain.Models.Commands.AuthCommands;

public class RequestResetCommand
{
    public string Email { get; set; } = string.Empty;
}
