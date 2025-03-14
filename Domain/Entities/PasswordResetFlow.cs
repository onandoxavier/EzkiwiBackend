using VirtualQueueApi.Domain.Entities;

namespace VirtualQueueApi.Models.Entities;

public class PasswordResetFlow : EntityBase<Guid>
{    
    public string CodeHash { get; set; } = string.Empty;
    public DateTimeOffset CodeExpiration { get; set; } = DateTime.UtcNow.AddMinutes(10);

    public int AttemptsCount { get; set; } = 0;//(quantas vezes usuário já tentou validar o código)
    public int RequestsCountInWindow { get; set; } = 1;//(quantas vezes foi solicitado novo código dentro de uma janela de 30min)

    public DateTimeOffset? BlockedUntil { get; set; }//(se a conta está bloqueada para este fluxo, guardar até quando)
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public DateTimeOffset WindowStartUtc { get; set; }//(hora em que começou a contar essa janela) ==> createdAt

    public string Ip { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;

    public int UserId { get; set; }
    public User User { get; set; }

    public PasswordResetFlow() { }
    public PasswordResetFlow(int userId, string codeHash, string ip, string userAgent)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CodeHash = codeHash;
        UserAgent = userAgent;
        Ip = ip;
    }
}
