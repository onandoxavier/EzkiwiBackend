using VirtualQueueApi.Domain.Entities;
using VirtualQueueApi.Domain.Models.Commands.AuthCommands;
using VirtualQueueApi.Domain.Models.Enum;

namespace VirtualQueueApi.Models.Entities;

public class User : EntityBase<int>
{
    public Guid ExternalId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public EUserRole Role { get; set; } = EUserRole.None;
    public int CompanyId { get; set; }
    public Company Company { get; set; }

    public User() { }

    public User(RegisterCommand command, Company company, string password, EUserRole role)
    {
        Name = command.Name;
        Email = command.Email;
        Password = password;
        Role = role;

        Company = company;
    }

    public void UpdatePassword(string newPassword)
    {
        string salt = BCrypt.Net.BCrypt.GenerateSalt();
        Password = BCrypt.Net.BCrypt.HashPassword(newPassword, salt);
        
        UpdateMe();
    }
}
