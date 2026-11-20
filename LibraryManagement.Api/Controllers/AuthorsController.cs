using LibraryManagement.Api.DTOs.Authors;
using LibraryManagement.Api.Filters;
using LibraryManagement.Api.RequestParameters;
using LibraryManagement.Api.Services.Contracts;

using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[ServiceFilter(typeof(LogActionFilter))]
[Route("api/[controller]")]
public class AuthorsController(
    IAuthorService authorService) : WebApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadAuthorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReadAuthorDto>>> GetAll(
        [FromQuery] PaginationParameters pagination)
    {
        var result = await authorService.GetAllAsync(pagination);

        if (result.IsError)
        {
            return Problem(result.FirstError);
        }

        Response.Headers.Append("X-Total-Count", result.Value.TotalCount.ToString());
        Response.Headers.Append("X-Total-Pages", result.Value.TotalPages.ToString());

        return Ok(result.Value.Items);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadAuthorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadAuthorDetailDto>> GetById(int id)
    {
        var result = await authorService.GetByIdAsync(id);

        if (result.IsError)
        {
            return Problem(result.FirstError);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReadAuthorDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadAuthorDetailDto>> Create(CreateAuthorDto createDto)
    {
        var result = await authorService.CreateAsync(createDto);

        if (result.IsError)
        {
            return Problem(result.FirstError);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value.Id },
            result.Value);
    }


    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReadAuthorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadAuthorDetailDto>> Update(int id, UpdateAuthorDto updateDto)
    {
        var result = await authorService.UpdateAsync(id, updateDto);

        if (result.IsError)
        {
            return Problem(result.FirstError);
        }

        return Ok(result.Value);
    }

    [HttpPatch("{id:int}")]
    [Consumes("application/json-patch+json")]
    [ProducesResponseType(typeof(ReadAuthorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadAuthorDetailDto>> Patch(
        int id,
        JsonPatchDocument<PatchAuthorDto> patchDocument)
    {
        var result = await authorService.PatchAsync(id, patchDocument);

        if (result.IsError)
        {
            return Problem(result.FirstError);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await authorService.DeleteAsync(id);

        if (result.IsError)
        {
            return Problem(result.FirstError);
        }

        return NoContent();
    }
}
