using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Common;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _service;

    public SemestersController(ISemesterService service)
    {
        _service = service;
    }

    private static SemesterResponse ToResponse(SemesterBusinessModel m) => new()
    {
        SemesterId = m.SemesterId,
        SemesterName = m.SemesterName,
        StartDate = m.StartDate,
        EndDate = m.EndDate
    };

    private static SemesterBusinessModel FromCreate(SemesterCreateRequest r) => new()
    {
        SemesterName = r.SemesterName,
        StartDate = r.StartDate,
        EndDate = r.EndDate
    };

    private static SemesterBusinessModel FromUpdate(SemesterUpdateRequest r) => new()
    {
        SemesterName = r.SemesterName,
        StartDate = r.StartDate,
        EndDate = r.EndDate
    };

    /// <summary>List semesters with search, sort, paging, field selection.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
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

        return Ok(PagedApiResponse<SemesterResponse>.Ok(
            responses, result.Data.Page, result.Data.PageSize, result.Data.TotalItems));
    }

    /// <summary>Get a semester by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] string? fields = null)
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

        return Ok(ApiResponse<SemesterResponse>.Ok(response));
    }

    /// <summary>Create a new semester.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] SemesterCreateRequest request)
    {
        var result = await _service.CreateAsync(FromCreate(request));
        if (!result.Success || result.Data == null)
            return BadRequest(ApiResponse<object>.Fail(result.Message));

        var response = ToResponse(result.Data);
        return CreatedAtAction(nameof(GetById), new { id = response.SemesterId },
            ApiResponse<SemesterResponse>.Ok(response, "Semester created"));
    }

    /// <summary>Update an existing semester.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] SemesterUpdateRequest request)
    {
        var result = await _service.UpdateAsync(id, FromUpdate(request));
        if (!result.Success || result.Data == null)
            return NotFound(ApiResponse<object>.NotFound(result.Message));

        return Ok(ApiResponse<SemesterResponse>.Ok(ToResponse(result.Data), "Semester updated"));
    }

    /// <summary>Delete a semester.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result.Success)
            return NotFound(ApiResponse<object>.NotFound(result.Message));
        return Ok(ApiResponse<object>.Ok(new { deleted = true }, result.Message));
    }
}
