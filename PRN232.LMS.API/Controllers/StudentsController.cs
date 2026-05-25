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
[Produces("application/json")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;

    public StudentsController(IStudentService service)
    {
        _service = service;
    }

    private static StudentResponse ToResponse(StudentBusinessModel m) => new()
    {
        StudentId = m.StudentId,
        FullName = m.FullName,
        Email = m.Email,
        DateOfBirth = m.DateOfBirth
    };

    private static StudentBusinessModel FromCreate(StudentCreateRequest r) => new()
    {
        FullName = r.FullName,
        Email = r.Email,
        DateOfBirth = r.DateOfBirth
    };

    private static StudentBusinessModel FromUpdate(StudentUpdateRequest r) => new()
    {
        FullName = r.FullName,
        Email = r.Email,
        DateOfBirth = r.DateOfBirth
    };

    /// <summary>List students with search (name or email), sort, paging, field selection.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<StudentResponse>), StatusCodes.Status200OK)]
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

        return Ok(PagedApiResponse<StudentResponse>.Ok(
            responses, result.Data.Page, result.Data.PageSize, result.Data.TotalItems));
    }

    /// <summary>Get a student by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
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

        return Ok(ApiResponse<StudentResponse>.Ok(response));
    }

    /// <summary>Create a new student.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] StudentCreateRequest request)
    {
        var result = await _service.CreateAsync(FromCreate(request));
        if (!result.Success || result.Data == null)
            return BadRequest(ApiResponse<object>.Fail(result.Message));

        var response = ToResponse(result.Data);
        return CreatedAtAction(nameof(GetById), new { id = response.StudentId },
            ApiResponse<StudentResponse>.Ok(response, "Student created"));
    }

    /// <summary>Update an existing student.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] StudentUpdateRequest request)
    {
        var result = await _service.UpdateAsync(id, FromUpdate(request));
        if (!result.Success || result.Data == null)
            return NotFound(ApiResponse<object>.NotFound(result.Message));

        return Ok(ApiResponse<StudentResponse>.Ok(ToResponse(result.Data), "Student updated"));
    }

    /// <summary>Delete a student.</summary>
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
