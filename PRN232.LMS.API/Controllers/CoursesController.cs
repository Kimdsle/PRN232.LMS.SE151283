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
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;

    public CoursesController(ICourseService courseService, IEnrollmentService enrollmentService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
    }

    private static CourseResponse ToResponse(CourseBusinessModel m) => new()
    {
        CourseId = m.CourseId,
        CourseName = m.CourseName,
        SemesterId = m.SemesterId,
        SemesterName = m.SemesterName
    };

    private static EnrollmentResponse EnrollmentToResponse(EnrollmentBusinessModel m) => new()
    {
        EnrollmentId = m.EnrollmentId,
        StudentId = m.StudentId,
        StudentName = m.StudentName,
        CourseId = m.CourseId,
        CourseName = m.CourseName,
        EnrollDate = m.EnrollDate,
        Status = m.Status
    };

    private static CourseBusinessModel FromCreate(CourseCreateRequest r) => new()
    {
        CourseName = r.CourseName,
        SemesterId = r.SemesterId
    };

    private static CourseBusinessModel FromUpdate(CourseUpdateRequest r) => new()
    {
        CourseName = r.CourseName,
        SemesterId = r.SemesterId
    };

    /// <summary>List courses with search, sort, paging, field selection, expand=semester.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] ListQueryOptions options)
    {
        var result = await _courseService.ListAsync(options);
        if (!result.Success || result.Data == null)
            return BadRequest(ApiResponse<object>.Fail(result.Message));

        var responses = result.Data.Items.Select(ToResponse).ToList();

        if (!string.IsNullOrWhiteSpace(options.Fields))
        {
            var shaped = responses.Select(r => QueryHelper.ApplyFields(r, options.Fields)).ToList();
            return Ok(PagedApiResponse<IDictionary<string, object?>>.Ok(
                shaped, result.Data.Page, result.Data.PageSize, result.Data.TotalItems));
        }

        return Ok(PagedApiResponse<CourseResponse>.Ok(
            responses, result.Data.Page, result.Data.PageSize, result.Data.TotalItems));
    }

    /// <summary>Get a course by id, optionally expand=semester.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? fields = null)
    {
        var result = await _courseService.GetByIdAsync(id);
        if (!result.Success || result.Data == null)
            return NotFound(ApiResponse<object>.NotFound(result.Message));

        var response = ToResponse(result.Data);

        if (!string.IsNullOrWhiteSpace(fields))
        {
            var shaped = QueryHelper.ApplyFields(response, fields);
            return Ok(ApiResponse<IDictionary<string, object?>>.Ok(shaped));
        }

        return Ok(ApiResponse<CourseResponse>.Ok(response));
    }

    /// <summary>
    /// NESTED RESOURCE: list enrollments belonging to a given course.
    /// Required by instructor: /courses/{id}/enrollments?expand=student
    /// </summary>
    [HttpGet("{id:int}/enrollments")]
    [ProducesResponseType(typeof(PagedApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListEnrollmentsByCourse(
        int id,
        [FromQuery] ListQueryOptions options)
    {
        var courseCheck = await _courseService.GetByIdAsync(id);
        if (!courseCheck.Success || courseCheck.Data == null)
            return NotFound(ApiResponse<object>.NotFound($"Course {id} not found."));

        var result = await _enrollmentService.ListByCourseAsync(id, options);

        if (!result.Success || result.Data == null)
            return BadRequest(ApiResponse<object>.Fail(result.Message));

        var responses = result.Data.Items.Select(EnrollmentToResponse).ToList();

        if (!string.IsNullOrWhiteSpace(options.Fields))
        {
            var shaped = responses.Select(r => QueryHelper.ApplyFields(r, options.Fields)).ToList();
            return Ok(PagedApiResponse<IDictionary<string, object?>>.Ok(
                shaped, result.Data.Page, result.Data.PageSize, result.Data.TotalItems));
        }

        return Ok(PagedApiResponse<EnrollmentResponse>.Ok(
            responses, result.Data.Page, result.Data.PageSize, result.Data.TotalItems));
    }

    /// <summary>Create a new course.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CourseCreateRequest request)
    {
        var result = await _courseService.CreateAsync(FromCreate(request));
        if (!result.Success || result.Data == null)
            return BadRequest(ApiResponse<object>.Fail(result.Message));

        var response = ToResponse(result.Data);
        return CreatedAtAction(nameof(GetById), new { id = response.CourseId },
            ApiResponse<CourseResponse>.Ok(response, "Course created"));
    }

    /// <summary>Update an existing course.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CourseUpdateRequest request)
    {
        var result = await _courseService.UpdateAsync(id, FromUpdate(request));
        if (!result.Success || result.Data == null)
            return NotFound(ApiResponse<object>.NotFound(result.Message));

        return Ok(ApiResponse<CourseResponse>.Ok(ToResponse(result.Data), "Course updated"));
    }

    /// <summary>Delete a course.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _courseService.DeleteAsync(id);
        if (!result.Success)
            return NotFound(ApiResponse<object>.NotFound(result.Message));
        return Ok(ApiResponse<object>.Ok(new { deleted = true }, result.Message));
    }
}
