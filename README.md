# PRN232 LMS API — Lab 1

ASP.NET Core 8 RESTful API for a Learning Management System, built on a 3-layer architecture with full search/sort/paging/expand support and Docker deployment.

**Course:** PRN232 — Cross-Platform Application Programming with .NET  
**Student:** Dai Kim Nguyen (SE151283)  
**Email:** nguyendkse151283@fpt.edu.vn  
**Repository:** https://github.com/Kimdsle/PRN232.LMS.SE151283

---

## 🚀 Quick Start (Docker)

The fastest way to run this project end-to-end:

```powershell
git clone https://github.com/Kimdsle/PRN232.LMS.SE151283.git
cd PRN232.LMS.SE151283\src
docker compose up -d --build
```

After cloning, the solution and `docker-compose.yml` live in the `src/` folder, so all
docker compose commands must be run from `PRN232.LMS.SE151283\src`.

Secrets are optional: `docker-compose.yml` ships with safe `:-default` values for the SA
password and JWT secret, so the stack comes up with **no `.env` required**. To override
them, create an `.env` file in `src/` (the `.env` file is NOT committed for security):

```powershell
# Use any strong SA password matching SQL Server's complexity policy
"LMS_SA_PASSWORD=YourStrong@Password2026!" | Out-File -Encoding UTF8 .env
```

Wait ~60 seconds for SQL Server to initialize and seed data to load, then open Swagger UI at
http://localhost:8080/swagger.

After startup:
- API:                  http://localhost:8080
- Swagger UI:           http://localhost:8080/swagger
- SQL Server (port):    localhost:1433 (sa / your .env password)

To stop the stack while keeping data:
```powershell
docker compose stop
```

To remove containers + data:
```powershell
docker compose down -v
```

---

## 🏗️ Architecture

The solution follows a strict **3-layer architecture** with one-way dependencies:
┌─────────────────────────────────────────────────────────┐
│  PRN232.LMS.API           (Presentation / Controllers)  │
│  • Controllers, ApiResponse envelope                    │
│  • Request / Response models (DTOs)                     │
│  • Swagger / OpenAPI configuration                      │
└────────────────────────┬────────────────────────────────┘
│ uses
▼
┌─────────────────────────────────────────────────────────┐
│  PRN232.LMS.Services      (Business Logic)              │
│  • Business models                                      │
│  • QueryHelper (search / sort / fields / expand)        │
│  • Entity ↔ BusinessModel mapping                       │
└────────────────────────┬────────────────────────────────┘
│ uses
▼
┌─────────────────────────────────────────────────────────┐
│  PRN232.LMS.Repositories  (Data Access)                 │
│  • Entity models                                        │
│  • LmsDbContext + Fluent API + HasData seed             │
│  • GenericRepository + 5 entity repositories            │
└────────────────────────┬────────────────────────────────┘
│ uses
▼
┌─────────────────────────────────────────────────────────┐
│  SQL Server 2022 (Docker container)                     │
│  • Database: PRN232_LMS                                 │
│  • Seed: 5 semesters, 50 students, 10 subjects,         │
│          20 courses, 500 enrollments                    │
└─────────────────────────────────────────────────────────┘

The four model types per lab spec:

| Type            | Lives in                           | Used for                                |
|-----------------|------------------------------------|------------------------------------------|
| Entity          | PRN232.LMS.Repositories\Entities   | EF Core database mapping                 |
| Business Model  | PRN232.LMS.Services\Models         | Internal domain processing               |
| Request Model   | PRN232.LMS.API\Models\Requests     | Client input (POST / PUT bodies)         |
| Response Model  | PRN232.LMS.API\Models\Responses    | API output (JSON returned to clients)    |

---

## 📡 API Endpoints

All endpoints are available with **both `/api/` and no-prefix routes** (e.g., `/api/students` and `/students` both work).

### Resources (5 main entities)

| Resource     | List          | Single        | Create        | Update           | Delete           |
|--------------|---------------|---------------|---------------|------------------|------------------|
| Semesters    | GET /api/semesters    | GET /api/semesters/{id}   | POST /api/semesters    | PUT /api/semesters/{id}   | DELETE /api/semesters/{id}   |
| Courses      | GET /api/courses      | GET /api/courses/{id}     | POST /api/courses      | PUT /api/courses/{id}     | DELETE /api/courses/{id}     |
| Subjects     | GET /api/subjects     | GET /api/subjects/{id}    | POST /api/subjects     | PUT /api/subjects/{id}    | DELETE /api/subjects/{id}    |
| Students     | GET /api/students     | GET /api/students/{id}    | POST /api/students     | PUT /api/students/{id}    | DELETE /api/students/{id}    |
| Enrollments  | GET /api/enrollments  | GET /api/enrollments/{id} | POST /api/enrollments  | PUT /api/enrollments/{id} | DELETE /api/enrollments/{id} |

### Nested resource

| Path                                              | Description                              |
|---------------------------------------------------|------------------------------------------|
| `GET /courses/{id}/enrollments?expand=student`    | List all enrollments for a given course, with related student info |

### List query options (supported on every collection endpoint)

| Parameter   | Example                          | Meaning                                            |
|-------------|----------------------------------|----------------------------------------------------|
| `?search`   | `?search=nguyen`                 | Case-insensitive substring across searchable fields |
| `?sort`     | `?sort=fullName,-dateOfBirth`    | Comma-separated, `-` prefix = descending           |
| `?page`     | `?page=2`                        | Page number (1-based, default 1)                   |
| `?size`     | `?size=20`                       | Page size (default 10)                             |
| `?fields`   | `?fields=studentId,fullName`     | Field projection — only return listed properties   |
| `?expand`   | `?expand=student,course`         | Include related entities in the response           |

### Response envelope

Every successful response follows this shape:

```json
{
  "success": true,
  "message": "Request processed successfully",
  "data": { ... },
  "errors": null
}
```

Collection responses add a `pagination` block:

```json
{
  "success": true,
  "data": [ ... ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 100,
    "totalPages": 10
  }
}
```

### HTTP status codes used

| Code | Used when                                          |
|------|----------------------------------------------------|
| 200  | Successful GET / PUT                               |
| 201  | Successful POST (resource created)                 |
| 204  | Successful DELETE (no content)                     |
| 400  | Bad request (invalid input)                        |
| 404  | Resource not found                                 |
| 500  | Unhandled server error                             |

---

## 🛠️ Technology Stack

- **.NET 8** (LTS) — ASP.NET Core Web API
- **Entity Framework Core 8** — ORM, code-first migrations, HasData seeding
- **SQL Server 2022 Express** — Database (runs in Docker)
- **System.Linq.Dynamic.Core 1.7.1** — Dynamic LINQ for runtime sort parsing
- **Swashbuckle.AspNetCore 6.6.2** — Swagger/OpenAPI documentation
- **Docker + Docker Compose** — Containerization + multi-service orchestration

---

## 📂 Solution Layout
PRN232.LMS.SE151283/
├── docker-compose.yml             ← orchestrates SQL Server + API
├── .env.example                   ← template for required environment variables
├── .gitignore                     ← excludes .env and build artifacts
├── README.md                      ← this file
├── PRN232.LMS.sln                 ← solution file
│
├── PRN232.LMS.API/                ← Presentation layer
│   ├── Controllers/               ← 5 controllers, dual routes per controller
│   ├── Models/
│   │   ├── Requests/              ← 10 request DTOs (Create + Update per entity)
│   │   └── Responses/             ← 5 response DTOs
│   ├── Common/                    ← ApiResponse, PagedApiResponse, PaginationMetadata
│   ├── Dockerfile                 ← multi-stage image
│   └── Program.cs                 ← DI registration, Swagger config, auto-migration
│
├── PRN232.LMS.Services/           ← Business logic layer
│   ├── Models/                    ← 5 business models + ListQueryOptions + BusinessResult + PagedResult
│   ├── Interfaces/                ← 5 service interfaces
│   ├── Services/                  ← 5 service implementations with entity↔business mapping
│   └── Helpers/
│       └── QueryHelper.cs         ← ApplySearch, ApplySort, ApplyFields, ShouldExpand
│
└── PRN232.LMS.Repositories/       ← Data access layer
├── Entities/                  ← 5 EF Core entities
├── Data/
│   └── LmsDbContext.cs        ← DbContext + Fluent API + seed (5/50/10/20/500)
├── Interfaces/                ← 6 repository interfaces (1 generic + 5 entity-specific)
├── Repositories/              ← GenericRepository<T> + 5 concrete repositories
└── Migrations/                ← InitialCreate migration with full seed data

---

## ✅ Lab Requirements Coverage

| # | Requirement                                                                          | Where it lives                                          |
|---|--------------------------------------------------------------------------------------|---------------------------------------------------------|
| 1 | 3-layer architecture (Controllers / Services / Repositories)                         | Three separate projects with one-way references         |
| 2 | Project naming convention `PRN232.[Name].API/Services/Repositories`                  | Solution structure                                      |
| 3 | DB schema + 5 / 50 / 10 / 20 / 500 seed                                              | LmsDbContext.SeedData + InitialCreate migration         |
| 4 | Four model types (Entity, Business, Request, Response)                               | Separate folders in their respective layers             |
| 5 | RESTful naming (plural nouns, resource-based URLs)                                   | All controllers use plural resource paths               |
| 6 | GET by ID with related data and 404 handling                                         | Every controller has GetById returning NotFound on null |
| 7 | List API: search + sort + paging + field selection + expansion                       | QueryHelper + ListQueryOptions + per-entity Includes    |
| 8 | Pagination metadata (page / pageSize / totalItems / totalPages)                      | PaginationMetadata + PagedApiResponse                   |
| 9 | Consistent response envelope                                                         | ApiResponse + PagedApiResponse                          |
| 10| Docker deployment (DB + API in containers)                                           | docker-compose.yml + Dockerfile                         |
| 11| Swagger / OpenAPI                                                                    | AddSwaggerGen with metadata, XML docs, tag grouping     |

---

## 🔐 Security Notes

The repository follows the **`.env` pattern** for secrets:
- `.env` is **never committed** (listed in `.gitignore`).
- `.env.example` shows the variable names without real values.
- `docker-compose.yml` reads `${LMS_SA_PASSWORD}` at run time.

Before running for the first time, copy the example file and set your own password:
```powershell
Copy-Item .env.example .env
notepad .env   # replace placeholder with a strong password
```

---

## 📝 License

Created as an academic submission for PRN232 Lab 1.
