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
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _service;

    public EnrollmentsController(IEnrollmentService service)
    {
        _service = service;
    }

    private static EnrollmentResponse ToResponse(EnrollmentBusinessModel m) => new()
    {
        EnrollmentId = m.EnrollmentId,
        StudentId = m.StudentId,
        StudentName = m.StudentName,
        CourseId = m.CourseId,
        CourseName = m.CourseName,
        EnrollDate = m.EnrollDate,
        Status = m.Status
    };

    private static EnrollmentBusinessModel FromCreate(EnrollmentCreateRequest r) => new()
    {
        StudentId = r.StudentId,
        CourseId = r.CourseId,
        EnrollDate = r.EnrollDate,
        Status = r.Status
    };

    private static EnrollmentBusinessModel FromUpdate(EnrollmentUpdateRequest r) => new()
    {
        StudentId = r.StudentId,
        CourseId = r.CourseId,
        EnrollDate = r.EnrollDate,
        Status = r.Status
    };

    /// <summary>
    /// List enrollments with search, sort, paging, field selection.
    /// Supports expand=student and/or expand=course.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
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

        return Ok(PagedApiResponse<EnrollmentResponse>.Ok(
            responses, result.Data.Page, result.Data.PageSize, result.Data.TotalItems));
    }

    /// <summary>Get an enrollment by id with related student and course.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
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

        return Ok(ApiResponse<EnrollmentResponse>.Ok(response));
    }

    /// <summary>Create a new enrollment.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] EnrollmentCreateRequest request)
    {
        var result = await _service.CreateAsync(FromCreate(request));
        if (!result.Success || result.Data == null)
            return BadRequest(ApiResponse<object>.Fail(result.Message));

        var response = ToResponse(result.Data);
        return CreatedAtAction(nameof(GetById), new { id = response.EnrollmentId },
            ApiResponse<EnrollmentResponse>.Ok(response, "Enrollment created"));
    }

    /// <summary>Update an existing enrollment.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] EnrollmentUpdateRequest request)
    {
        var result = await _service.UpdateAsync(id, FromUpdate(request));
        if (!result.Success || result.Data == null)
            return NotFound(ApiResponse<object>.NotFound(result.Message));

        return Ok(ApiResponse<EnrollmentResponse>.Ok(ToResponse(result.Data), "Enrollment updated"));
    }

    /// <summary>Delete an enrollment.</summary>
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
