namespace VirtualQueueApi.Domain.Models.Commands.AuthCommands;

public class RegisterCommand
{
    public string Name { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
