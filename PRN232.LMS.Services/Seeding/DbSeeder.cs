using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Services.Seeding;

public static class DbSeeder
{
    public static void SeedUsers(LmsDbContext db)
    {
        var added = false;
        if (!db.Users.Any(u => u.Username == "admin"))
        {
            db.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = "Admin"
            });
            Console.WriteLine("[Seed] Admin user created");
            added = true;
        }
        if (!db.Users.Any(u => u.Username == "user"))
        {
            db.Users.Add(new User
            {
                Username = "user",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = "Student"
            });
            Console.WriteLine("[Seed] Regular user (Student) created");
            added = true;
        }
        if (added) db.SaveChanges();
    }
}
