namespace VirtualQueueApi.Domain.Models.Results.AuthResults;

public class ForgotPasswordResult
{
    public string Token { get; set; } = Guid.NewGuid().ToString("N");
    public string Message { get; set; } = "If this email exists, instructions have been sent.";
}
