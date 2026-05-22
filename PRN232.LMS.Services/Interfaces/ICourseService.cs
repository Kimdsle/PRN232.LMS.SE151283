using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface ICourseService
{
    Task<BusinessResult<CourseBusinessModel>> GetByIdAsync(int id);
    Task<BusinessResult<PagedResult<CourseBusinessModel>>> ListAsync(ListQueryOptions options);
    Task<BusinessResult<CourseBusinessModel>> CreateAsync(CourseBusinessModel model);
    Task<BusinessResult<CourseBusinessModel>> UpdateAsync(int id, CourseBusinessModel model);
    Task<BusinessResult<bool>> DeleteAsync(int id);
}
