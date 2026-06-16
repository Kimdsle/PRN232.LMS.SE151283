using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repo;

    public StudentService(IStudentRepository repo)
    {
        _repo = repo;
    }

    private static StudentBusinessModel ToBusiness(Student e) => new()
    {
        StudentId = e.StudentId,
        FullName = e.FullName,
        Email = e.Email,
        DateOfBirth = e.DateOfBirth,
        Phone = e.Phone
    };

    private static Student ToEntity(StudentBusinessModel m) => new()
    {
        StudentId = m.StudentId,
        FullName = m.FullName,
        Email = m.Email,
        DateOfBirth = m.DateOfBirth,
        Phone = m.Phone
    };

    public async Task<BusinessResult<StudentBusinessModel>> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            return BusinessResult<StudentBusinessModel>.NotFound($"Student {id} not found.");
        return BusinessResult<StudentBusinessModel>.Ok(ToBusiness(entity));
    }

    public async Task<BusinessResult<PagedResult<StudentBusinessModel>>> ListAsync(ListQueryOptions options)
    {
        var query = _repo.Query();

        query = QueryHelper.ApplySearch(query, options.Search, x => x.FullName, x => x.Email);
        query = QueryHelper.ApplySort(query, options.Sort, "StudentId");

        var total = await query.CountAsync();
        var items = await query
            .Skip((options.Page - 1) * options.Size)
            .Take(options.Size)
            .ToListAsync();

        var paged = new PagedResult<StudentBusinessModel>
        {
            Items = items.Select(ToBusiness).ToList(),
            Page = options.Page,
            PageSize = options.Size,
            TotalItems = total
        };
        return BusinessResult<PagedResult<StudentBusinessModel>>.Ok(paged);
    }

    public async Task<BusinessResult<StudentBusinessModel>> CreateAsync(StudentBusinessModel model)
    {
        var entity = ToEntity(model);
        entity.StudentId = 0;
        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return BusinessResult<StudentBusinessModel>.Ok(ToBusiness(entity), "Student created.");
    }

    public async Task<BusinessResult<StudentBusinessModel>> UpdateAsync(int id, StudentBusinessModel model)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return BusinessResult<StudentBusinessModel>.NotFound($"Student {id} not found.");
        existing.FullName = model.FullName;
        existing.Email = model.Email;
        existing.DateOfBirth = model.DateOfBirth;
        existing.Phone = model.Phone;
        await _repo.UpdateAsync(existing);
        await _repo.SaveChangesAsync();
        return BusinessResult<StudentBusinessModel>.Ok(ToBusiness(existing), "Student updated.");
    }

    public async Task<BusinessResult<bool>> DeleteAsync(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        if (!ok) return BusinessResult<bool>.NotFound($"Student {id} not found.");
        await _repo.SaveChangesAsync();
        return BusinessResult<bool>.Ok(true, "Student deleted.");
    }
}
