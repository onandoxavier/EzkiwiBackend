namespace VirtualQueueApi.Domain.Models.Commands.UserCommands;

public class UpdateUserProfileCommand
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
 }

