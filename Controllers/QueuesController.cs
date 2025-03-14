using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VirtualQueueApi.Data;
using VirtualQueueApi.Models.Entities;
using VirtualQueueApi.Domain.Models.Results.QueueResults;
using VirtualQueueApi.Domain.Models.Commands.QueueCommands;
using VirtualQueueApi.Utils.Extensions;
using VirtualQueueApi.Utils.Exceptions;

namespace VirtualQueueApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QueuesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public QueuesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<QueueListResult>>> GetQueues()
    {
        var externalCompanyId = User.GetCompanyId();
        var company = _context.Companies.FirstOrDefault(x => x.ExternalId == externalCompanyId);

        if (company == null) throw new BusinessException("Company not found");

        var queues = await _context.Queues.Where(q => q.CompanyId == company.Id && !q.Deleted).ToListAsync();

        if (!queues.Any()) return NotFound();

        var result = new List<QueueListResult>();

        queues.ForEach(q=>result.Add(new QueueListResult(q.ExternalId, q.Name)));

        return result;
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetQueue(Guid id)
    {
        var queue = await _context.Queues.FirstOrDefaultAsync(q => q.ExternalId == id);
        if (queue == null)
            return NotFound();

        var company = await _context.Companies.FindAsync(queue.CompanyId);
        if (company == null)
            return NotFound();

        var result = new QueueResult(queue.ExternalId, queue.Name, company.Name);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateQueue(CreateQueueCommand command)
    {
        var isValidSubscription = User.IsValidSubscription();

        var externalCompanyId = User.GetCompanyId();
        var company = _context.Companies.FirstOrDefault(x => x.ExternalId == externalCompanyId);

        if (company == null) throw new BusinessException("Company not found");

        if (!isValidSubscription)
        {
            var totalQueues = await _context.Queues.CountAsync(q => q.CompanyId == company.Id);
            if (totalQueues >= 3) throw new BusinessException("On free plan you can create only 3 queues.");
        }

        var queue = new Queue() { CompanyId = company.Id, Name = command.Name };
        _context.Queues.Add(queue);

        await _context.SaveChangesAsync();

        return Ok(queue);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQueue(Guid id)
    {
        var isValidSubscription = User.IsValidSubscription();
        if (!isValidSubscription) throw new BusinessException("You can't delete queue on free plan.");

        var externalCompanyId = User.GetCompanyId();
        var company = _context.Companies.FirstOrDefault(x => x.ExternalId == externalCompanyId);

        if (company == null) throw new BusinessException("Company not found");

        var queue = await _context.Queues.FirstOrDefaultAsync(q => q.ExternalId == id && q.CompanyId == company.Id);
        if (queue == null)
            return NotFound();

        _context.Queues.Remove(queue);
        await _context.SaveChangesAsync();
        return Ok();
    }
}
