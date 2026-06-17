using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Services.Seeding;

public static class DbSeeder
{
    public static void EnsureAdminUser(LmsDbContext db)
    {
        if (!db.Users.Any(u => u.Username == "admin"))
        {
            db.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = "Admin"
            });
            db.SaveChanges();
            Console.WriteLine("[Seed] Admin user created");
        }
        else
        {
            Console.WriteLine("[Seed] Admin user already present");
        }
    }
}
