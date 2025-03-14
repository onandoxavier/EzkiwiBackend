using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using VirtualQueueApi.Data;
using VirtualQueueApi.Domain.Contracts.Services;
using VirtualQueueApi.Domain.Models.Results.AuthResults;
using VirtualQueueApi.Domain.Models.Commands.AuthCommands;
using VirtualQueueApi.Utils.Extensions;

namespace VirtualQueueApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authenticationService;

    public AuthController(IAuthService authenticationService,
        ApplicationDbContext context, IConfiguration configuration)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("Register", Name = "Register")]
    public async Task<ActionResult<RegisterResult>> Register([FromBody] RegisterCommand command, CancellationToken ct = default)
    {
        var result = await _authenticationService.Register(command);
        return Ok(result);
    }

    [HttpPost("Login", Name = "Login")]
    public async Task<ActionResult<TokenResult>> Login([FromBody] LoginCommand command, CancellationToken ct = default)
    {
        var token = await _authenticationService.Login(command, ct);
        return Ok(token);
    }

    [HttpPost("Refresh", Name ="RefreshToken")]
    public async Task<ActionResult<TokenResult>> RefreshToken([FromBody] RefreshRequest model, CancellationToken ct = default)
    {
        var result = await _authenticationService.RefreshToken(model, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("Logout", Name = "Logout")]
    public async Task<ActionResult<TokenResult>> Logout(CancellationToken ct = default)
    {
        var userId = User.GetId();
        var tokenId = User.GetTokenId();
        await _authenticationService.Logout(userId, tokenId, ct);

        return Ok();
    }

    [HttpPost("ForgotPassword", Name = "ForgotPassword")]
    public async Task<ActionResult<ForgotPasswordResult>> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken ct = default)
    {
        command.Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        command.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var result = await _authenticationService.ForgotPassword(command, ct);
        return Ok(result);
    }

    [HttpPost("ResendCode", Name = "ResendCode")]
    public async Task<ActionResult<string>> ResendCode([FromBody] ResendCodeCommand command, CancellationToken ct = default)
    {
        var result = await _authenticationService.ResendCode(command, ct);
        return Ok(result);
    }

    [HttpPost("VerifyCode", Name = "VerifyCode")]
    public async Task<ActionResult<string>> ValidateCode(ValidateCodeCommand command, CancellationToken ct = default)
    {
        var result = await _authenticationService.ValidateCode(command, ct);
        return Ok(result);
    }

    [HttpPost("ConfirmReset", Name = "ConfirmReset")]
    public async Task<ActionResult<string>> ConfirmReset(ConfirmResetCommand command, CancellationToken ct = default)
    {
        var result = await _authenticationService.ConfirmReset(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("UpdatePassword", Name = "UpdatePassword")]
    public async Task<ActionResult<string>> UpdatePassword([FromBody] UpdatePasswordCommand command, CancellationToken ct = default)
    {
        var userId = User.GetId();
        var result = await _authenticationService.UpdateUserPassword(command, userId, ct);
        
        return Ok(result);
    }
}
