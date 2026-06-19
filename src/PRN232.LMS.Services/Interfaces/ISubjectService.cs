using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface ISubjectService
{
    Task<BusinessResult<SubjectBusinessModel>> GetByIdAsync(int id);
    Task<BusinessResult<PagedResult<SubjectBusinessModel>>> ListAsync(ListQueryOptions options);
    Task<BusinessResult<SubjectBusinessModel>> CreateAsync(SubjectBusinessModel model);
    Task<BusinessResult<SubjectBusinessModel>> UpdateAsync(int id, SubjectBusinessModel model);
    Task<BusinessResult<bool>> DeleteAsync(int id);
}
