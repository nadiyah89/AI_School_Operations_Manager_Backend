using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolOperations.AI.Orchestration;
using SchoolOperations.DTOs.AI;

namespace SchoolOperations.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly AIOrchestrator _orchestrator;

    public ChatController(
        AIOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }


    [HttpPost]
    public async Task<IActionResult> Chat(
        [FromBody] ChatRequestDto dto)
    {
        var authHeader =
            Request.Headers.Authorization.ToString();

        var accessToken = string.Empty;

        if (authHeader.StartsWith(
            "Bearer ",
            StringComparison.OrdinalIgnoreCase))
        {
            accessToken =
                authHeader["Bearer ".Length..].Trim();
        }


        var response =
            await _orchestrator.ProcessAsync(
                dto.Message,
                accessToken);


        return Ok(
            new
            {
                response
            });
    }
}