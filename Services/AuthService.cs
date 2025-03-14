using System.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity.Data;
using VirtualQueueApi.Utils.Exceptions;
using VirtualQueueApi.Models.Entities;
using VirtualQueueApi.Domain.Entities;
using VirtualQueueApi.Domain.Contracts.Repositories;
using VirtualQueueApi.Domain.Contracts.Transactions;
using VirtualQueueApi.Domain.Contracts.Services;
using VirtualQueueApi.Domain.Models.Enum;
using VirtualQueueApi.Domain.Models.Results.AuthResults;
using VirtualQueueApi.Domain.Models.Commands.AuthCommands;
using VirtualQueueApi.ExternalServices;

namespace VirtualQueueApi.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly IQueueRepository _queueRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordResetFlowRepository _passwordResetFlowRepository;
    private readonly ISubscriptionManagementRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(IConfiguration configuration,
        IUserRepository userRepository,
        IQueueRepository queueRepository,
        ICompanyRepository companyRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetFlowRepository passwordResetFlowRepository,
        ISubscriptionManagementRepository subscriptionRepository,
        IUnitOfWork unitOfWork)
    {
        _configuration = configuration;
        _userRepository = userRepository;
        _queueRepository = queueRepository;
        _companyRepository = companyRepository;
        _subscriptionRepository = subscriptionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordResetFlowRepository = passwordResetFlowRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterResult> Register(RegisterCommand command, CancellationToken ct = default)
    {
        if (await _userRepository.HasAny(c => c.Email == command.Email, ct))            
            throw new BusinessException("Email already registered.");            

        string salt = BCrypt.Net.BCrypt.GenerateSalt();
        var password = BCrypt.Net.BCrypt.HashPassword(command.Password, salt);        

        var company = new Company(command.CompanyName);
        var user = new User(command, company, password, EUserRole.Owner);
        var queue = new Queue(name: "First Queue", company: company);

        await _userRepository.Add(user, ct);
        await _companyRepository.Add(company, ct);
        await _queueRepository.Add(queue, ct);

        await _unitOfWork.Commit(ct);

        var (accessToken, refreshToken) = await GenerateTokensAsync(user);

        await _unitOfWork.Commit(ct);

#if !DEBUG
        EmailService.SendWelcomeEmail(user.Email, user.Name, company.Name);
#endif

        var result = new RegisterResult(token: accessToken, refreshToken: refreshToken, queueId: queue.ExternalId);

        return result;        
    }

    public async Task<TokenResult> Login(LoginCommand command, CancellationToken ct = default)
    {
        var user = await _userRepository.Get(
            expression: c => c.Email == command.Email,
            include: source => source.Include(c => c.Company), 
            ct: ct);

        if (user == null || !BCrypt.Net.BCrypt.Verify(command.Password, user.Password))        
            throw new UnauthorizedException("Invalid email or password.");

        var subscription = await _subscriptionRepository.Get(
            expression: s => s.CompanyId == user.CompanyId && !s.Deleted && s.Status == ESubscriptionStatus.Active);

        var subscriptionDate = subscription?.End;

        var (accessToken, refreshToken) = await GenerateTokensAsync(user, subscriptionDate);
        
        await _unitOfWork.Commit();

        var result = new TokenResult(accessToken, refreshToken);

        return result;
    }

    public async Task<TokenResult> RefreshToken(RefreshRequest model, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(model.RefreshToken))
            throw new BusinessException("Session expired.");

        var refreshTokenHash = HashRefreshToken(model.RefreshToken);
        var storedToken = await _refreshTokenRepository.Get(expression:rt => rt.TokenHash == refreshTokenHash, ct: ct);

        if (storedToken == null)
            throw new UnauthorizedAccessException("Session expired.");

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Session expired.");

        if (storedToken.Used || storedToken.Invalidated)
            throw new UnauthorizedAccessException("Session expired.");

        storedToken.Used = true;
        await _unitOfWork.Commit();

        var user = await _userRepository.Get(
            expression: c => c.Id == storedToken.UserId,
            include: source => source.Include(c => c.Company),
            ct: ct);

        if (user == null)
            throw new UnauthorizedAccessException("Session expired.");

        var subscription = await _subscriptionRepository.Get(
            expression: s => s.CompanyId == user.CompanyId && !s.Deleted && s.Status == ESubscriptionStatus.Active);

        var subscriptionDate = subscription?.End;

        var (accessToken, refreshToken) = await GenerateTokensAsync(user, subscriptionDate);

        await _unitOfWork.Commit();

        var result = new TokenResult(accessToken, refreshToken);

        return result;
    }

    public async Task Logout(int userId, Guid tokenId, CancellationToken ct)
    {
        var storedToken = await _refreshTokenRepository.Get(expression: rt => rt.UserId == userId && rt.JwtId == tokenId, ct: ct);

        if (storedToken == null)
            throw new UnauthorizedAccessException("Session expired.");

        storedToken.Invalidated = true;
        await _unitOfWork.Commit();
    }

    public async Task<ForgotPasswordResult> ForgotPassword(ForgotPasswordCommand command, CancellationToken ct = default)
    {
        var result = new ForgotPasswordResult();

        var user = await _userRepository.Get(expression: c => c.Email == command.Email, ct: ct);
        if (user == null)        
            return result;               
                
        var existingFlow = await _passwordResetFlowRepository.Get(expression: x => x.UserId == user.Id && x.CompletedAtUtc == null, ct: ct);
        if (existingFlow != null && existingFlow.BlockedUntil > DateTime.UtcNow)
            return result;               
   
        var code = Generate6DigitCode();
#if DEBUG
        Console.WriteLine($"Code: {code}");
#endif
        var codeHash = ComputeSha256Hash(code);
        var flow = new PasswordResetFlow(user.Id, codeHash, command.Ip, command.UserAgent);

        await _passwordResetFlowRepository.Add(flow);
        await _unitOfWork.Commit(ct);

#if !DEBUG
        EmailService.SendResetCode(user.Email, user.Name, code);
#endif
        result.Token = flow.Id.ToString("N");

        return result;
    }

    public async Task<string> ResendCode(ResendCodeCommand command, CancellationToken ct = default)
    {
        if (!Guid.TryParse(command.Token, out Guid flowId))        
            throw new Exception("If this email exists, instructions have been sent.");

        var flow = await _passwordResetFlowRepository.Get(expression: flow => flow.Id == flowId, ct: ct, include: source => source.Include(flow => flow.User));        
        if (flow == null)
            return "If this email exists, instructions have been sent.";

        if (flow.BlockedUntil.HasValue && flow.BlockedUntil.Value > DateTime.UtcNow)
        {
            var totalHours = flow.BlockedUntil.Value.Subtract(DateTime.UtcNow).TotalHours;
            throw new BusinessException($"You have exceeded the number of attempts allowed, please wait {totalHours} hours to try again.");
        }

        var now = DateTime.UtcNow;
        if (now - flow.WindowStartUtc > TimeSpan.FromMinutes(30))
        {
            flow.RequestsCountInWindow = 1;
            flow.WindowStartUtc = now;
        }
        else
        {
            flow.RequestsCountInWindow++;
            if (flow.RequestsCountInWindow > 3)
            {
                flow.BlockedUntil = now.AddHours(24);
                throw new BusinessException("You have exceeded the number of attempts allowed, please wait 24 hours to try again.");                
            }
        }

        var newCode = Generate6DigitCode();
        var newCodeHash = ComputeSha256Hash(newCode);

#if DEBUG
        Console.WriteLine($"Code: {newCode}");
#endif

        flow.CodeHash = newCodeHash;
        flow.CodeExpiration = now.AddMinutes(10);
        flow.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Commit(ct);        
        
        EmailService.SendResetCode(flow.User.Email, flow.User.Name, newCode);

        return "If this email exists, instructions have been sent.";
    }

    public async Task<string> ValidateCode(ValidateCodeCommand command, CancellationToken ct = default)
    {
        if (!Guid.TryParse(command.Token, out Guid flowId))
            throw new Exception("Invalid code or request not found.");

        var flow = await _passwordResetFlowRepository.Get(expression: flow => flow.Id == flowId, ct: ct);        
        if (flow == null)
            return "Invalid code or request not found.";

        if (flow.BlockedUntil.HasValue && flow.BlockedUntil.Value > DateTime.UtcNow)
        {
            var totalHours = flow.BlockedUntil.Value.Subtract(DateTime.UtcNow).TotalHours;
            throw new BusinessException($"You have exceeded the number of attempts allowed, please wait {totalHours} hours to try again.");
        }

        if (flow.CodeExpiration < DateTime.UtcNow)
            throw new BusinessException("Code expired. Request a new code.");        

        if (flow.AttemptsCount >= 3)    
            throw new BusinessException("Too many invalid attempts, request a new code.");

        var submittedCodeHash = ComputeSha256Hash(command.Code);
        if (submittedCodeHash != flow.CodeHash)
        {
            flow.AttemptsCount++;
            throw new BusinessException("Invalid code.");
        }

        flow.CompletedAtUtc = DateTime.UtcNow;
        flow.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Commit(ct);

        return "Code validated successfully!";
    }

    public async Task<string> ConfirmReset(ConfirmResetCommand command, CancellationToken ct = default)
    {
        if (!Guid.TryParse(command.Token, out Guid flowId))
            throw new Exception("Invalid code or request not found.");
                
        var flow = await _passwordResetFlowRepository.Get(expression: flow => flow.Id == flowId, ct: ct);        
        if (flow == null)
            return "Invalid code or request not found.";

        if (flow.CompletedAtUtc == null)
            throw new BusinessException("Invalid code or request not found.");

        if (flow.CompletedAtUtc.Value.AddMinutes(30) < DateTime.UtcNow)
            throw new BusinessException("Code expired. Request a new code.");

        var user = await _userRepository.GetById(flow.UserId);
        if (user == null)
            throw new BusinessException("User not found.");

        user.UpdatePassword(command.NewPassword);        

        await _unitOfWork.Commit(ct);

        return "Password reset successfully!";
    }

    public async Task<string> UpdateUserPassword(UpdatePasswordCommand command, int userId, CancellationToken ct = default)
    {        
        var user = await _userRepository.GetById(userId);
        if (user == null)
            throw new BusinessException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(command.CurrentPassword, user.Password))
            throw new BusinessException("Invalid password.");

        user.UpdatePassword(command.NewPassword);

        await _unitOfWork.Commit(ct);

        return "Password reset successfully!";
    }
    
    private string ComputeSha256Hash(string input)
    {
        using var sha256 = SHA256.Create();

        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
    private string Generate6DigitCode()
    {
        var randomNumber = new byte[4];
        RandomNumberGenerator.Fill(randomNumber);
        var numericValue = BitConverter.ToUInt32(randomNumber, 0) % 1_000_000; // fica entre 0 e 999999
        var code = numericValue.ToString("D6"); // força 6 dígitos com zeros à esquerda
        
        return code;
    }
    public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(User user, DateTimeOffset? subscriptionLimit = null)
    {
        var (accessToken, jti) = GenerateJwtToken(user, subscriptionLimit);

        var rawRefreshToken = GenerateRefreshTokenValue(); 
        var refreshTokenHash = HashRefreshToken(rawRefreshToken);

        var refreshTokenEntity = new RefreshToken(refreshTokenHash, jti, user);

        await _refreshTokenRepository.Add(refreshTokenEntity);        
        return (accessToken, rawRefreshToken);
    }

    private (string, Guid) GenerateJwtToken(User user, DateTimeOffset? subscriptionLimit = null)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var jti = Guid.NewGuid();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("CompanyId", user.Company.ExternalId.ToString()),
            new Claim("CompanyName", user.Company.Name),
            new Claim("SubscriptionLimit", subscriptionLimit?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") ?? ""), //ISO8601
            new Claim(JwtRegisteredClaimNames.Jti, jti.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), jti);
    }
    private string GenerateRefreshTokenValue(int size = 32)
    {
        var randomNumber = new byte[size];
        RandomNumberGenerator.Fill(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    private string HashRefreshToken(string tokenValue)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(tokenValue));
        return Convert.ToHexString(hashedBytes).ToLowerInvariant();
    }
}
