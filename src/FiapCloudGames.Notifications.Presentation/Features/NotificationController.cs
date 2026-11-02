using FiapCloudGames.BuildingBlocks.Web;
using FiapCloudGames.Notifications.Application.Features.Get;
using FiapCloudGames.Notifications.Application.Features.Retry;
using FiapCloudGames.Notifications.Presentation.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FiapCloudGames.Notifications.Presentation.Features;

[ApiController]
[Route("internal/v1/notifications")]
public sealed class NotificationController : ControllerBase
{
    [HttpGet("{notificationId:guid}")]
    [Authorize(Policy = NotificationsPolicies.NotificationsRead)]
    [ProducesResponseType<NotificationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized,
        FiapApiDefaultsExtensions.ProblemJsonContentType)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status403Forbidden,
        FiapApiDefaultsExtensions.ProblemJsonContentType)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound,
        FiapApiDefaultsExtensions.ProblemJsonContentType)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status500InternalServerError,
        FiapApiDefaultsExtensions.ProblemJsonContentType)]
    public async Task<ActionResult<NotificationResponse>> GetAsync(
        Guid notificationId,
        [FromServices] GetNotificationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(notificationId, cancellationToken);
        return Ok(NotificationResponse.FromResult(result));
    }

    [HttpPost("{notificationId:guid}/retry")]
    [Authorize(Policy = NotificationsPolicies.NotificationsRetry)]
    [ProducesResponseType<NotificationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized,
        FiapApiDefaultsExtensions.ProblemJsonContentType)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status403Forbidden,
        FiapApiDefaultsExtensions.ProblemJsonContentType)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound,
        FiapApiDefaultsExtensions.ProblemJsonContentType)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict,
        FiapApiDefaultsExtensions.ProblemJsonContentType)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status500InternalServerError,
        FiapApiDefaultsExtensions.ProblemJsonContentType)]
    public async Task<ActionResult<NotificationResponse>> RetryAsync(
        Guid notificationId,
        [FromServices] RetryNotificationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(notificationId, cancellationToken);
        return Ok(NotificationResponse.FromResult(result));
    }
}
