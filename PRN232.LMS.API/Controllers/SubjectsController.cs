using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Common;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("[controller]")]
[Produces("application/json")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _service;

    public SubjectsController(ISubjectService service)
    {
        _service = service;
    }

    private static SubjectResponse ToResponse(SubjectBusinessModel m) => new()
    {
        SubjectId = m.SubjectId,
        SubjectCode = m.SubjectCode,
        SubjectName = m.SubjectName,
        Credit = m.Credit
    };

    private static SubjectBusinessModel FromCreate(SubjectCreateRequest r) => new()
    {
        SubjectCode = r.SubjectCode,
        SubjectName = r.SubjectName,
        Credit = r.Credit
    };

    private static SubjectBusinessModel FromUpdate(SubjectUpdateRequest r) => new()
    {
        SubjectCode = r.SubjectCode,
        SubjectName = r.SubjectName,
        Credit = r.Credit
    };

    /// <summary>List subjects with search (name or code), sort, paging, field selection.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] ListQueryOptions options)
    {
        var result = await _service.ListAsync(options);
        if (!result.Success || result.Data == null)
            return BadRequest(ApiResponse<object>.Fail(result.Message));

        var responses = result.Data.Items.Select(ToResponse).ToList();

        if (!string.IsNullOrWhiteSpace(options.Fields))
        {
            var shaped = responses.Select(r => QueryHelper.ApplyFields(r, options.Fields)).ToList();
            return Ok(PagedApiResponse<IDictionary<string, object?>>.Ok(
                shaped, result.Data.Page, result.Data.PageSize, result.Data.TotalItems));
        }

        return Ok(PagedApiResponse<SubjectResponse>.Ok(
            responses, result.Data.Page, result.Data.PageSize, result.Data.TotalItems));
    }

    /// <summary>Get a subject by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? fields = null)
    {
        var result = await _service.GetByIdAsync(id);
        if (!result.Success || result.Data == null)
            return NotFound(ApiResponse<object>.NotFound(result.Message));

        var response = ToResponse(result.Data);

        if (!string.IsNullOrWhiteSpace(fields))
        {
            var shaped = QueryHelper.ApplyFields(response, fields);
            return Ok(ApiResponse<IDictionary<string, object?>>.Ok(shaped));
        }

        return Ok(ApiResponse<SubjectResponse>.Ok(response));
    }

    /// <summary>Create a new subject.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] SubjectCreateRequest request)
    {
        var result = await _service.CreateAsync(FromCreate(request));
        if (!result.Success || result.Data == null)
            return BadRequest(ApiResponse<object>.Fail(result.Message));

        var response = ToResponse(result.Data);
        return CreatedAtAction(nameof(GetById), new { id = response.SubjectId },
            ApiResponse<SubjectResponse>.Ok(response, "Subject created"));
    }

    /// <summary>Update an existing subject.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] SubjectUpdateRequest request)
    {
        var result = await _service.UpdateAsync(id, FromUpdate(request));
        if (!result.Success || result.Data == null)
            return NotFound(ApiResponse<object>.NotFound(result.Message));

        return Ok(ApiResponse<SubjectResponse>.Ok(ToResponse(result.Data), "Subject updated"));
    }

    /// <summary>Delete a subject.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result.Success)
            return NotFound(ApiResponse<object>.NotFound(result.Message));
        return Ok(ApiResponse<object>.Ok(new { deleted = true }, result.Message));
    }
}
