using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface IStudentService
{
    Task<BusinessResult<StudentBusinessModel>> GetByIdAsync(int id);
    Task<BusinessResult<PagedResult<StudentBusinessModel>>> ListAsync(ListQueryOptions options);
    Task<BusinessResult<StudentBusinessModel>> CreateAsync(StudentBusinessModel model);
    Task<BusinessResult<StudentBusinessModel>> UpdateAsync(int id, StudentBusinessModel model);
    Task<BusinessResult<bool>> DeleteAsync(int id);
}
