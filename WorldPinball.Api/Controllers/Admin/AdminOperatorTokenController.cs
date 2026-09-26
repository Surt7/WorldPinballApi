using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorldPinball.Api.Contracts.Machine;
using WorldPinball.Application.Features.Machines.Admin.CreateOperatorToken;
using WorldPinball.Application.Features.Machines.Admin.CreateOperatorToken.Commands;

namespace WorldPinball.Api.Controllers.Admin;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/v1/admin/operator-tokens")]
public sealed class AdminOperatorTokensController(
    ICreateOperatorTokenHandler handler,
    IValidator<CreateOperatorTokenRequest> validator
) : ControllerBase
{

    [HttpPost]
    [EndpointSummary("Creates a new operator token")]
    [ProducesResponseType(typeof(CreateOperatorTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CreateOperatorTokenResponse>> Create(
        [FromBody] CreateOperatorTokenRequest request,
        CancellationToken ct)
    {
        var issuedByUser = User.FindFirstValue("sub") ?? "admin";
        
        
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            foreach (var failure in validationResult.Errors)
            {
                ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
            }
            return ValidationProblem(ModelState);
        }
        
        var command = new CreateOperatorTokenCommand(
            request.Name,
            request.Scopes ?? "*",
            request.ValidityDays,
            issuedByUser);

        var result = await handler.HandleAsync(command, ct);
        
        
        return result switch
        {
            CreateOperatorTokenResult.Success success => Ok(new CreateOperatorTokenResponse(
                success.TokenId,
                success.Name,
                success.RawToken,
                success.Prefix,
                success.Scopes,
                success.ExpiresAtUtc)),
            
            CreateOperatorTokenResult.InvalidScopes invalid => BadRequest(new
            {
                message = invalid.Reason
            }),
            
            _ => throw new InvalidOperationException($"Unsupported result: {result.GetType().Name}")
            
        };
    }
    
    
}