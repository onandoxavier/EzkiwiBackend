namespace VirtualQueueApi.Domain.Models.Results.AuthResults;

public class RegisterResult
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public Guid QueueId { get; set; } = Guid.Empty;

    public RegisterResult(string token, string refreshToken, Guid queueId)
    {
        Token = token;
        QueueId = queueId;
        RefreshToken = refreshToken;
    }
}
