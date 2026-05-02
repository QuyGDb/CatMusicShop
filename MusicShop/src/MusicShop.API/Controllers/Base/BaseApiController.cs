using Microsoft.AspNetCore.Mvc;
using MusicShop.API.Infrastructure;
using MusicShop.Application.Common;
using MusicShop.Domain.Common;

namespace MusicShop.API.Controllers.Base;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> HandleResult<T>(Result<T> result)
    {
        if (result.IsFailure) return MapError(result.Error);

        return Ok(ApiResponse<T>.SuccessResult(result.Value));
    }

    protected IActionResult HandleNoContentResult(Result result)
    {
        if (result.IsFailure) return MapError(result.Error);

        return NoContent();
    }

    protected ActionResult<ApiResponse<IReadOnlyList<T>>> HandlePaginatedResult<T>(Result<PaginatedResult<T>> result)
    {
        if (result.IsFailure) return MapError(result.Error);

        MetaData meta = new MetaData
        {
            Page = result.Value.PageNumber,
            Limit = result.Value.PageSize,
            Total = result.Value.TotalCount
        };

        return Ok(ApiResponse<IReadOnlyList<T>>.SuccessResult(result.Value.Items, meta));
    }

    protected ActionResult<ApiResponse<T>> HandleCreatedResult<T>(Result<T> result, string actionName, Func<T, object> routeValuesProvider)
    {
        if (result.IsFailure) return MapError(result.Error);

        return CreatedAtAction(actionName, routeValuesProvider(result.Value), ApiResponse<T>.SuccessResult(result.Value));
    }

    protected ActionResult MapError(Error error)
    {
        int statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem(
            statusCode: statusCode,
            title: error.Code,
            detail: error.Message,
            type: GetErrorType(statusCode)
        );
    }

    private static string GetErrorType(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
        StatusCodes.Status403Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.11",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };
}
