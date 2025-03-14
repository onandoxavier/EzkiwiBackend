using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using VirtualQueueApi.Data;
using VirtualQueueApi.Hubs;
using VirtualQueueApi.Models.Entities;
using VirtualQueueApi.Domain.Models.Results.AuthResults;
using VirtualQueueApi.Domain.Models.Commands.PasswordCommands;
using VirtualQueueApi.Utils.Extensions;
using VirtualQueueApi.Utils.Exceptions;

namespace VirtualQueueApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/queues/{queueId}/[controller]")]
    public class PasswordsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<QueueHub> _hubContext;
        private readonly ILogger<PasswordsController> _logger;

        public PasswordsController(ApplicationDbContext context, IHubContext<QueueHub> hubContext, ILogger<PasswordsController> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet(Name ="GetPasswords")]
        public async Task<IActionResult> GetPasswords(Guid queueId)
        {
            var queue = await _context.Queues.FirstOrDefaultAsync(q => q.ExternalId == queueId);
            if (queue == null) return NotFound();

            var passwords = await _context.PasswordHistories
                .Where(p => p.QueueId == queue.Id)
                .OrderByDescending(p => p.UpdatedAt)
                .Take(10)
                .ToListAsync();

            var result = new List<NewPasswordResult>();

            passwords.ForEach(pw => result.Add(new NewPasswordResult(pw.Id, pw.CreatedAt, pw.Value, queue.ExternalId)));

            return Ok(result);
        }

        [HttpGet("Highest", Name = "GetHighestPasswords")]
        public async Task<IActionResult> GetHighestPasswords(Guid queueId)
        {
            var companyExternalId = User.GetCompanyId();
            var company = await _context.Companies.FirstOrDefaultAsync(x => x.ExternalId == companyExternalId);
            if (company == null) throw new BusinessException("Company not found.");

            var queue = await _context.Queues.FirstOrDefaultAsync(q => q.ExternalId == queueId && company.Id == q.CompanyId);
            if (queue == null) throw new BusinessException("Queue not found.");

            var highestPassword = await _context.PasswordHistories
                .FromSqlRaw(@"
                    SELECT TOP 1 *
                    FROM PasswordHistories 
                    WHERE QueueId = {0} AND TRY_CAST([Value] AS INT) IS NOT NULL
                    ORDER BY TRY_CAST([Value] as int) DESC                 
                ", queue.Id)
                .FirstOrDefaultAsync();

            return Ok(highestPassword);
        }

        [HttpPost(Name = "CallNewPassword")]
        public async Task<IActionResult> CallNewPassword(Guid queueId, [FromBody] CreatePasswordCommand command)
        {
            var isValidSubscription = User.IsValidSubscription();

            var companyExternalId = User.GetCompanyId();
            var company = await _context.Companies.FirstOrDefaultAsync(x => x.ExternalId == companyExternalId);

            if (company == null) throw new BusinessException("Company not found.");

            var queue = await _context.Queues.FirstOrDefaultAsync(q => q.ExternalId == queueId && q.CompanyId == company.Id);
            if (queue == null) throw new BusinessException("Queue not found.");

            if (!isValidSubscription)
            {
                var totalPasswords = await _context.PasswordHistories.Where(p => p.QueueId == queue.Id).CountAsync();
                if (totalPasswords >= 50) 
                    throw new BusinessException("On free plan you can call only 50 numbers/names.");
            }

            var newPassword = new PasswordHistory
            {
                Value = command.Password,
                QueueId = queue.Id
            };

            _context.PasswordHistories.Add(newPassword);

            newPassword.UpdateMe();
            await _context.SaveChangesAsync();

            var result = new NewPasswordResult(newPassword.Id, newPassword.UpdatedAt.Value, newPassword.Value, queue.ExternalId);

            // Notify clients via SignalR
            _logger.LogInformation("Sending message to group {QueueId} with data: {Result}", queueId, result.ToString());
            await _hubContext.Clients.Group(queueId.ToString()).SendAsync("ReceivePassword", result);

            return Ok(result);
        }

        [HttpPut("Restart", Name = "RestartQueue")]
        public async Task<IActionResult> RestartPassword(Guid queueId)
        {
            var isValidSubscription = User.IsValidSubscription();
            if (!isValidSubscription) throw new BusinessException("You can't restart queue on free plan.");

            var companyExternalId = User.GetCompanyId();
            var company = await _context.Companies.FirstOrDefaultAsync(x => x.ExternalId == companyExternalId);

            if (company == null) throw new BusinessException("Company not found.");

            var queue = await _context.Queues
                .Where(q => q.ExternalId == queueId && q.CompanyId == company.Id)
                .Include(q => q.PasswordHistories)
                .FirstOrDefaultAsync();

            if (queue == null) throw new BusinessException("Queue not found.");

            _context.PasswordHistories.RemoveRange(queue.PasswordHistories);
            await _context.SaveChangesAsync();

            // Notify clients via SignalR
            await _hubContext.Clients.Group(queueId.ToString()).SendAsync("RestartPassword");

            return NoContent();
        }

        [HttpPut("{id}", Name = "RecallPassword")]
        public async Task<IActionResult> RecallPassword(int id, Guid queueId)
        {
            var isValidSubscription = User.IsValidSubscription();
            if (!isValidSubscription) throw new BusinessException("You can't recall password on free plan.");

            var password = await _context.PasswordHistories.FindAsync(id);
            if (password == null) return NotFound();

            password.UpdateMe();

            await _context.SaveChangesAsync();

            var result = new NewPasswordResult(password.Id, password.UpdatedAt.Value, password.Value, queueId);

            // Notify clients via SignalR
            await _hubContext.Clients.Group(queueId.ToString()).SendAsync("RecallPassword", result);

            return Ok();
        }

        [HttpDelete("{id}", Name = "DeletePassword")]
        public async Task<IActionResult> DeletePassword(int id, Guid queueId)
        {
            var isValidSubscription = User.IsValidSubscription();
            if (!isValidSubscription) throw new BusinessException("You can't delete password on free plan.");

            var external = Guid.Parse(User.FindFirst("CompanyId").Value);

            var company = await _context.Companies.FirstOrDefaultAsync(x => x.ExternalId == external);
            if (company == null) throw new BusinessException("Company not found.");

            var queue = await _context.Queues.FirstOrDefaultAsync(q => q.ExternalId == queueId && q.CompanyId == company.Id);
            if (queue == null) throw new BusinessException("Queue not found.");

            var password = await _context.PasswordHistories.FindAsync(id);
            if (password == null) return NotFound();

            _context.PasswordHistories.Remove(password);
            await _context.SaveChangesAsync();

            // Notify clients via SignalR
            await _hubContext.Clients.Group(queueId.ToString()).SendAsync("RemovePassword", password.Id);

            return NoContent();
        }
    }
}
