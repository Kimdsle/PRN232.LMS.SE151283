using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Services;

public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _repo;

    public SemesterService(ISemesterRepository repo)
    {
        _repo = repo;
    }

    private static SemesterBusinessModel ToBusiness(Semester e) => new()
    {
        SemesterId = e.SemesterId,
        SemesterName = e.SemesterName,
        StartDate = e.StartDate,
        EndDate = e.EndDate
    };

    private static Semester ToEntity(SemesterBusinessModel m) => new()
    {
        SemesterId = m.SemesterId,
        SemesterName = m.SemesterName,
        StartDate = m.StartDate,
        EndDate = m.EndDate
    };

    public async Task<BusinessResult<SemesterBusinessModel>> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            return BusinessResult<SemesterBusinessModel>.NotFound($"Semester {id} not found.");
        return BusinessResult<SemesterBusinessModel>.Ok(ToBusiness(entity));
    }

    public async Task<BusinessResult<PagedResult<SemesterBusinessModel>>> ListAsync(ListQueryOptions options)
    {
        var query = _repo.Query();
        var total = await query.CountAsync();
        var items = await query
            .OrderBy(x => x.SemesterId)
            .Skip((options.Page - 1) * options.Size)
            .Take(options.Size)
            .ToListAsync();

        var paged = new PagedResult<SemesterBusinessModel>
        {
            Items = items.Select(ToBusiness).ToList(),
            Page = options.Page,
            PageSize = options.Size,
            TotalItems = total
        };
        return BusinessResult<PagedResult<SemesterBusinessModel>>.Ok(paged);
    }

    public async Task<BusinessResult<SemesterBusinessModel>> CreateAsync(SemesterBusinessModel model)
    {
        var entity = ToEntity(model);
        entity.SemesterId = 0;
        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return BusinessResult<SemesterBusinessModel>.Ok(ToBusiness(entity), "Semester created.");
    }

    public async Task<BusinessResult<SemesterBusinessModel>> UpdateAsync(int id, SemesterBusinessModel model)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return BusinessResult<SemesterBusinessModel>.NotFound($"Semester {id} not found.");
        existing.SemesterName = model.SemesterName;
        existing.StartDate = model.StartDate;
        existing.EndDate = model.EndDate;
        await _repo.UpdateAsync(existing);
        await _repo.SaveChangesAsync();
        return BusinessResult<SemesterBusinessModel>.Ok(ToBusiness(existing), "Semester updated.");
    }

    public async Task<BusinessResult<bool>> DeleteAsync(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        if (!ok) return BusinessResult<bool>.NotFound($"Semester {id} not found.");
        await _repo.SaveChangesAsync();
        return BusinessResult<bool>.Ok(true, "Semester deleted.");
    }
}
