using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Repositories.Repositories;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PRN232 LMS API",
        Version = "v1",
        Description = "ASP.NET Core 8 REST API for a Learning Management System. "
                    + "3-layer architecture with full search, sort, paging, field "
                    + "selection, and expansion support. Built for PRN232 Lab 1.",
        Contact = new OpenApiContact
        {
            Name  = "SE151283 - Dai Kim Nguyen",
            Email = "nguyendkse151283@fpt.edu.vn",
            Url   = new Uri("https://github.com/Kimdsle/PRN232.LMS.SE151283")
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    options.TagActionsBy(api =>
    {
        if (api.GroupName != null) return new[] { api.GroupName };
        var controllerName = api.ActionDescriptor.RouteValues["controller"];
        return new[] { controllerName ?? "Default" };
    });

    options.DocInclusionPredicate((name, api) => true);
});

builder.Services.AddDbContext<LmsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(15),
            errorNumbersToAdd: null)));

// Repository registrations (Phase 2)
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

// Service registrations (Phase 3)
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

var app = builder.Build();

// ───────────────────────────────────────────────────────────
// Auto-apply EF migrations on startup with retry (Phase 9)
// Ensures docker compose up creates schema + seeds 5/50/10/20/500
// ───────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();

    const int maxAttempts = 20;
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            logger.LogInformation("Applying database migrations (attempt {Attempt}/{Max})...", attempt, maxAttempts);
            db.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully.");
            break;
        }
        catch (Exception ex)
        {
            if (attempt == maxAttempts)
            {
                logger.LogError(ex, "Migration failed after {Max} attempts. Continuing without DB.", maxAttempts);
                break;
            }
            logger.LogWarning("DB not ready yet ({Message}). Waiting 5s before retry...", ex.Message);
            Thread.Sleep(5000);
        }
    }
}

// Swagger always on (in container we want it accessible)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PRN232 LMS API v1");
    c.DocumentTitle = "PRN232 LMS API — Swagger UI";
    c.DefaultModelsExpandDepth(-1);
});

// Disable HTTPS redirect inside container (we're behind nginx-less localhost)
// app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();
app.Run();
