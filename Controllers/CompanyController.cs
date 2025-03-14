using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtualQueueApi.Domain.Contracts.Services;
using VirtualQueueApi.Domain.Models.Commands.UserCommands;
using VirtualQueueApi.Domain.Models.Queries;
using VirtualQueueApi.Domain.Models.Results.UserResults;
using VirtualQueueApi.Utils.Extensions;

namespace VirtualQueueApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompanyController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public CompanyController(IMapper mapper,
        IUserService userService)
    {
        _userService = userService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetCompany(CancellationToken ct = default) 
    {
        var search = new UserSearch() { Id = User.GetId() };
        var result = await _userService.Get<UserResult>(search: search, ct: ct);

        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCompany([FromBody] UpdateUserProfileCommand command,  CancellationToken ct = default)
    {
        var user = await _userService.UpdateUserProfile(ct: ct, userId: User.GetId(), command: command);
        var result = _mapper.Map<UserResult>(user);

        return Ok(result);
    }
}
