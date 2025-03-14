namespace VirtualQueueApi.Domain.Models.Commands.AuthCommands;

public class ConfirmResetCommand
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
