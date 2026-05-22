using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repo;

    public CourseService(ICourseRepository repo)
    {
        _repo = repo;
    }

    private static CourseBusinessModel ToBusiness(Course e) => new()
    {
        CourseId = e.CourseId,
        CourseName = e.CourseName,
        SemesterId = e.SemesterId,
        SemesterName = e.Semester?.SemesterName
    };

    private static Course ToEntity(CourseBusinessModel m) => new()
    {
        CourseId = m.CourseId,
        CourseName = m.CourseName,
        SemesterId = m.SemesterId
    };

    public async Task<BusinessResult<CourseBusinessModel>> GetByIdAsync(int id)
    {
        var entity = await _repo.Query()
            .Include(c => c.Semester)
            .FirstOrDefaultAsync(c => c.CourseId == id);
        if (entity == null)
            return BusinessResult<CourseBusinessModel>.NotFound($"Course {id} not found.");
        return BusinessResult<CourseBusinessModel>.Ok(ToBusiness(entity));
    }

    public async Task<BusinessResult<PagedResult<CourseBusinessModel>>> ListAsync(ListQueryOptions options)
    {
        IQueryable<Course> query = _repo.Query();

        if (QueryHelper.ShouldExpand(options.Expand, "semester"))
            query = query.Include(c => c.Semester);

        query = QueryHelper.ApplySearch(query, options.Search, x => x.CourseName);
        query = QueryHelper.ApplySort(query, options.Sort, "CourseId");

        var total = await query.CountAsync();
        var items = await query
            .Skip((options.Page - 1) * options.Size)
            .Take(options.Size)
            .ToListAsync();

        var paged = new PagedResult<CourseBusinessModel>
        {
            Items = items.Select(ToBusiness).ToList(),
            Page = options.Page,
            PageSize = options.Size,
            TotalItems = total
        };
        return BusinessResult<PagedResult<CourseBusinessModel>>.Ok(paged);
    }

    public async Task<BusinessResult<CourseBusinessModel>> CreateAsync(CourseBusinessModel model)
    {
        var entity = ToEntity(model);
        entity.CourseId = 0;
        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return BusinessResult<CourseBusinessModel>.Ok(ToBusiness(entity), "Course created.");
    }

    public async Task<BusinessResult<CourseBusinessModel>> UpdateAsync(int id, CourseBusinessModel model)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return BusinessResult<CourseBusinessModel>.NotFound($"Course {id} not found.");
        existing.CourseName = model.CourseName;
        existing.SemesterId = model.SemesterId;
        await _repo.UpdateAsync(existing);
        await _repo.SaveChangesAsync();
        return BusinessResult<CourseBusinessModel>.Ok(ToBusiness(existing), "Course updated.");
    }

    public async Task<BusinessResult<bool>> DeleteAsync(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        if (!ok) return BusinessResult<bool>.NotFound($"Course {id} not found.");
        await _repo.SaveChangesAsync();
        return BusinessResult<bool>.Ok(true, "Course deleted.");
    }
}
