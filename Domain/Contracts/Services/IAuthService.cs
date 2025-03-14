using Microsoft.AspNetCore.Identity.Data;
using VirtualQueueApi.Domain.Models.Commands.AuthCommands;
using VirtualQueueApi.Domain.Models.Results.AuthResults;

namespace VirtualQueueApi.Domain.Contracts.Services;

public interface IAuthService
{
    Task<RegisterResult> Register(RegisterCommand command, CancellationToken ct = default);
    Task<TokenResult> Login(LoginCommand command, CancellationToken ct = default);
    Task<TokenResult> RefreshToken(RefreshRequest model, CancellationToken ct);
    Task Logout(int userId, Guid tokenId, CancellationToken ct);
    Task<ForgotPasswordResult> ForgotPassword(ForgotPasswordCommand command, CancellationToken ct = default);
    Task<string> ResendCode(ResendCodeCommand command, CancellationToken ct = default);
    Task<string> ValidateCode(ValidateCodeCommand command, CancellationToken ct = default);
    Task<string> ConfirmReset(ConfirmResetCommand command, CancellationToken ct = default);
    Task<string> UpdateUserPassword(UpdatePasswordCommand command, int userId, CancellationToken ct = default);
}
