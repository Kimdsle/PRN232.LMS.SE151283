using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _repo;

    public EnrollmentService(IEnrollmentRepository repo)
    {
        _repo = repo;
    }

    private static EnrollmentBusinessModel ToBusiness(Enrollment e) => new()
    {
        EnrollmentId = e.EnrollmentId,
        StudentId = e.StudentId,
        StudentName = e.Student?.FullName,
        CourseId = e.CourseId,
        CourseName = e.Course?.CourseName,
        EnrollDate = e.EnrollDate,
        Status = e.Status
    };

    private static Enrollment ToEntity(EnrollmentBusinessModel m) => new()
    {
        EnrollmentId = m.EnrollmentId,
        StudentId = m.StudentId,
        CourseId = m.CourseId,
        EnrollDate = m.EnrollDate,
        Status = m.Status
    };

    public async Task<BusinessResult<EnrollmentBusinessModel>> GetByIdAsync(int id)
    {
        var entity = await _repo.Query()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.EnrollmentId == id);
        if (entity == null)
            return BusinessResult<EnrollmentBusinessModel>.NotFound($"Enrollment {id} not found.");
        return BusinessResult<EnrollmentBusinessModel>.Ok(ToBusiness(entity));
    }

    public async Task<BusinessResult<PagedResult<EnrollmentBusinessModel>>> ListAsync(ListQueryOptions options)
    {
        IQueryable<Enrollment> query = _repo.Query();

        if (QueryHelper.ShouldExpand(options.Expand, "student"))
            query = query.Include(e => e.Student);
        if (QueryHelper.ShouldExpand(options.Expand, "course"))
            query = query.Include(e => e.Course);

        query = QueryHelper.ApplySearch(query, options.Search, x => x.Status);
        query = QueryHelper.ApplySort(query, options.Sort, "EnrollmentId");

        var total = await query.CountAsync();
        var items = await query
            .Skip((options.Page - 1) * options.Size)
            .Take(options.Size)
            .ToListAsync();

        var paged = new PagedResult<EnrollmentBusinessModel>
        {
            Items = items.Select(ToBusiness).ToList(),
            Page = options.Page,
            PageSize = options.Size,
            TotalItems = total
        };
        return BusinessResult<PagedResult<EnrollmentBusinessModel>>.Ok(paged);
    }

    public async Task<BusinessResult<EnrollmentBusinessModel>> CreateAsync(EnrollmentBusinessModel model)
    {
        var entity = ToEntity(model);
        entity.EnrollmentId = 0;
        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return BusinessResult<EnrollmentBusinessModel>.Ok(ToBusiness(entity), "Enrollment created.");
    }

    public async Task<BusinessResult<EnrollmentBusinessModel>> UpdateAsync(int id, EnrollmentBusinessModel model)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return BusinessResult<EnrollmentBusinessModel>.NotFound($"Enrollment {id} not found.");
        existing.StudentId = model.StudentId;
        existing.CourseId = model.CourseId;
        existing.EnrollDate = model.EnrollDate;
        existing.Status = model.Status;
        await _repo.UpdateAsync(existing);
        await _repo.SaveChangesAsync();
        return BusinessResult<EnrollmentBusinessModel>.Ok(ToBusiness(existing), "Enrollment updated.");
    }

    public async Task<BusinessResult<bool>> DeleteAsync(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        if (!ok) return BusinessResult<bool>.NotFound($"Enrollment {id} not found.");
        await _repo.SaveChangesAsync();
        return BusinessResult<bool>.Ok(true, "Enrollment deleted.");
    }
}
