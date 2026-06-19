using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface IEnrollmentService
{
    Task<BusinessResult<EnrollmentBusinessModel>> GetByIdAsync(int id);
    Task<BusinessResult<PagedResult<EnrollmentBusinessModel>>> ListAsync(ListQueryOptions options);
    Task<BusinessResult<PagedResult<EnrollmentBusinessModel>>> ListByCourseAsync(int courseId, ListQueryOptions options);
    Task<BusinessResult<EnrollmentBusinessModel>> CreateAsync(EnrollmentBusinessModel model);
    Task<BusinessResult<EnrollmentBusinessModel>> UpdateAsync(int id, EnrollmentBusinessModel model);
    Task<BusinessResult<bool>> DeleteAsync(int id);
}
