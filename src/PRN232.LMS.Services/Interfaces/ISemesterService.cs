using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface ISemesterService
{
    Task<BusinessResult<SemesterBusinessModel>> GetByIdAsync(int id);
    Task<BusinessResult<PagedResult<SemesterBusinessModel>>> ListAsync(ListQueryOptions options);
    Task<BusinessResult<SemesterBusinessModel>> CreateAsync(SemesterBusinessModel model);
    Task<BusinessResult<SemesterBusinessModel>> UpdateAsync(int id, SemesterBusinessModel model);
    Task<BusinessResult<bool>> DeleteAsync(int id);
}
