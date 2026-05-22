using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Repositories.Repositories.Base;

namespace PRN232.LMS.Repositories.Repositories;

public class SemesterRepository : GenericRepository<Semester>, ISemesterRepository
{
    public SemesterRepository(LmsDbContext context) : base(context) { }
}
