namespace VirtualQueueApi.Domain.Models.Commands.AuthCommands;

public class UpdatePasswordCommand
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
