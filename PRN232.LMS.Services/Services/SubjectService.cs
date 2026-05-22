using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Services;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _repo;

    public SubjectService(ISubjectRepository repo)
    {
        _repo = repo;
    }

    private static SubjectBusinessModel ToBusiness(Subject e) => new()
    {
        SubjectId = e.SubjectId,
        SubjectCode = e.SubjectCode,
        SubjectName = e.SubjectName,
        Credit = e.Credit
    };

    private static Subject ToEntity(SubjectBusinessModel m) => new()
    {
        SubjectId = m.SubjectId,
        SubjectCode = m.SubjectCode,
        SubjectName = m.SubjectName,
        Credit = m.Credit
    };

    public async Task<BusinessResult<SubjectBusinessModel>> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            return BusinessResult<SubjectBusinessModel>.NotFound($"Subject {id} not found.");
        return BusinessResult<SubjectBusinessModel>.Ok(ToBusiness(entity));
    }

    public async Task<BusinessResult<PagedResult<SubjectBusinessModel>>> ListAsync(ListQueryOptions options)
    {
        var query = _repo.Query();

        query = QueryHelper.ApplySearch(query, options.Search, x => x.SubjectName, x => x.SubjectCode);
        query = QueryHelper.ApplySort(query, options.Sort, "SubjectId");

        var total = await query.CountAsync();
        var items = await query
            .Skip((options.Page - 1) * options.Size)
            .Take(options.Size)
            .ToListAsync();

        var paged = new PagedResult<SubjectBusinessModel>
        {
            Items = items.Select(ToBusiness).ToList(),
            Page = options.Page,
            PageSize = options.Size,
            TotalItems = total
        };
        return BusinessResult<PagedResult<SubjectBusinessModel>>.Ok(paged);
    }

    public async Task<BusinessResult<SubjectBusinessModel>> CreateAsync(SubjectBusinessModel model)
    {
        var entity = ToEntity(model);
        entity.SubjectId = 0;
        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return BusinessResult<SubjectBusinessModel>.Ok(ToBusiness(entity), "Subject created.");
    }

    public async Task<BusinessResult<SubjectBusinessModel>> UpdateAsync(int id, SubjectBusinessModel model)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return BusinessResult<SubjectBusinessModel>.NotFound($"Subject {id} not found.");
        existing.SubjectCode = model.SubjectCode;
        existing.SubjectName = model.SubjectName;
        existing.Credit = model.Credit;
        await _repo.UpdateAsync(existing);
        await _repo.SaveChangesAsync();
        return BusinessResult<SubjectBusinessModel>.Ok(ToBusiness(existing), "Subject updated.");
    }

    public async Task<BusinessResult<bool>> DeleteAsync(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        if (!ok) return BusinessResult<bool>.NotFound($"Subject {id} not found.");
        await _repo.SaveChangesAsync();
        return BusinessResult<bool>.Ok(true, "Subject deleted.");
    }
}
