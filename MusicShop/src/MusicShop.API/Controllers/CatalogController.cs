using MediatR;
using Microsoft.AspNetCore.Mvc;
using MusicShop.API.Controllers.Base;
using MusicShop.API.Infrastructure;
using MusicShop.Domain.Common;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Application.UseCases.Catalog.CatalogSearch;

namespace MusicShop.API.Controllers;

public class CatalogController(IMediator mediator) : BaseApiController
{
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CatalogSearchResult>>> Search([FromQuery] string q)
    {
        Result<CatalogSearchResult> result = await mediator.Send(new SearchCatalogQuery(q));
        return HandleResult(result);
    }
}
