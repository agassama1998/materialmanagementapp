# Material Management Application Tutorial

A step-by-step guide to building an ASP.NET Core MVC application with Identity, Entity Framework Core, and Docker support.

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Project Setup](#project-setup)
3. [Database Configuration](#database-configuration)
4. [Creating Models](#creating-models)
5. [Setting Up DbContext](#setting-up-dbcontext)
6. [Configuring Identity](#configuring-identity)
7. [Creating Controllers](#creating-controllers)
8. [Seeding Data](#seeding-data)
9. [Docker Configuration](#docker-configuration)
10. [Git and GitHub Setup](#git-and-github-setup)

---

## Prerequisites

Before starting, ensure you have the following installed:
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional)
- A [GitHub](https://github.com/) account

---

## Project Setup

### Step 1: Create a New ASP.NET Core Web Application

Open your terminal/command prompt and run:

```bash
# Create a new directory for your project
mkdir MaterialManagement
cd MaterialManagement

# Create a new ASP.NET Core MVC project
dotnet new mvc -n MaterialManagement -au Individual
cd MaterialManagement
```

**What this does:**
- `dotnet new mvc` creates a new MVC project
- `-n MaterialManagement` names the project
- `-au Individual` adds individual user authentication (Identity)

### Step 2: Install Required NuGet Packages

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
dotnet add package Microsoft.AspNetCore.Identity.UI --version 8.0.0
dotnet add package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore --version 8.0.0
```

**Package purposes:**
- **Identity.EntityFrameworkCore**: User authentication and authorization
- **EntityFrameworkCore.SqlServer**: Database provider for SQL Server
- **EntityFrameworkCore.Tools**: Database migration tools
- **Identity.UI**: Pre-built UI for Identity pages
- **Diagnostics.EntityFrameworkCore**: Developer exception pages for EF Core

---

## Database Configuration

### Step 3: Configure Connection String

Open `appsettings.json` and add your connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MaterialManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Note for macOS/Linux users:** LocalDB is Windows-only. For cross-platform development, use SQLite or Docker SQL Server.

---

## Creating Models

### Step 4: Create the Category Model

Create a new folder called `Models` and add `Category.cs`:

```csharp
namespace MaterialManagement.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation property
        public ICollection<Material>? Materials { get; set; }
    }
}
```

### Step 5: Create the Material Model

In the `Models` folder, add `Material.cs`:

```csharp
namespace MaterialManagement.Models
{
    public class Material
    {
        public int MaterialId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        // Foreign key
        public int CategoryId { get; set; }

        // Navigation property
        public Category? Category { get; set; }
    }
}
```

**Key concepts:**
- **Primary Key**: `CategoryId` and `MaterialId` (convention: ClassName + Id)
- **Foreign Key**: `CategoryId` in Material links to Category
- **Navigation Properties**: Allow accessing related data

---

## Setting Up DbContext

### Step 6: Create ApplicationDbContext

Create a `Data` folder and add `ApplicationDbContext.cs`:

```csharp
using MaterialManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MaterialManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Material> Materials { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Material entity
            modelBuilder.Entity<Material>()
                .Property(m => m.Price)
                .HasColumnType("decimal(18,2)");

            // Configure relationships
            modelBuilder.Entity<Material>()
                .HasOne(m => m.Category)
                .WithMany(c => c.Materials)
                .HasForeignKey(m => m.CategoryId);
        }
    }
}
```

**DbContext responsibilities:**
- Manages database connection
- Tracks entity changes
- Executes queries
- Handles migrations

---

## Configuring Identity

### Step 7: Configure Services in Program.cs

Update `Program.cs`:

```csharp
using MaterialManagement.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedData.Initialize(services, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
```

**Key configuration:**
- **AddDbContext**: Registers database context
- **AddDefaultIdentity**: Configures user authentication
- **AddRoles**: Enables role-based authorization
- **UseAuthentication/UseAuthorization**: Adds auth middleware

---

## Creating Controllers

### Step 8: Create CategoriesController

In the `Controllers` folder, add `CategoriesController.cs`:

```csharp
using MaterialManagement.Data;
using MaterialManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaterialManagement.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Materials)
                .FirstOrDefaultAsync(m => m.CategoryId == id);

            if (category == null) return NotFound();

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Name,Description")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,Name,Description")] Category category)
        {
            if (id != category.CategoryId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);

            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}
```

### Step 9: Create MaterialsController

Similarly, create `MaterialsController.cs` following the same CRUD pattern, but include Category selection for the foreign key.

---

## Seeding Data

### Step 10: Create Seed Data Class

In the `Data` folder, add `SeedData.cs`:

```csharp
using MaterialManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace MaterialManagement.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles
            string[] roleNames = { "Admin", "Manager", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed Admin User
            var adminEmail = "admin@materialmanagement.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newAdmin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            // Seed sample data
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Add Categories if none exist
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Electronics", Description = "Electronic components and devices" },
                    new Category { Name = "Tools", Description = "Hand and power tools" },
                    new Category { Name = "Safety Equipment", Description = "Personal protective equipment" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
```

---

## Database Migration

### Step 11: Create and Apply Migrations

```bash
# Create initial migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

**Migration commands explained:**
- `migrations add`: Creates migration files
- `database update`: Applies migrations to database

---

## Docker Configuration

### Step 12: Create Dockerfile

Create a `Dockerfile` in the project root:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY MaterialManagement.csproj .
RUN dotnet restore "MaterialManagement.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "MaterialManagement.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "MaterialManagement.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published files from publish stage
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "MaterialManagement.dll"]
```

### Step 13: Create .dockerignore

Create a `.dockerignore` file:

```
**/bin
**/obj
**/.vs
**/.vscode
**/.git
**/.gitignore
**/docker-compose*
**/Dockerfile*
```

### Step 14: Build and Run Docker Container

```bash
# Build Docker image
docker build -t materialmanagement .

# Run container
docker run -p 8080:8080 -p 8081:8081 materialmanagement
```

---

## Git and GitHub Setup

### Step 15: Create .gitignore

Create a `.gitignore` file in your project root (use the comprehensive .NET gitignore template).

### Step 16: Initialize Git Repository

```bash
# Initialize git repository
git init

# Check status
git status
```

### Step 17: Create GitHub Repository

1. Go to [GitHub](https://github.com)
2. Click the **+** icon → **New repository**
3. Name it: `materialmanagementapp`
4. Keep it **Public** or **Private** (your choice)
5. **Do NOT** initialize with README, .gitignore, or license
6. Click **Create repository**

### Step 18: Add Remote and Push

```bash
# Add remote repository
git remote add origin https://github.com/YOUR_USERNAME/materialmanagementapp.git

# Add all files
git add .

# Create initial commit
git commit -m "Initial commit: Material Management application"

# Push to GitHub
git push -u origin main
```

**If you get an error about the branch name:**
```bash
# Rename branch to main
git branch -M main

# Push again
git push -u origin main
```

### Step 19: Verify on GitHub

1. Go to your repository URL
2. Verify all files are uploaded
3. Check that .gitignore is working (bin/, obj/ should not be committed)

---

## Running the Application

### Step 20: Run Locally

```bash
# Run the application
dotnet run

# Or with hot reload
dotnet watch run
```

Visit: `http://localhost:5000` or `https://localhost:5001`

### Step 21: Test the Application

1. **Register a new user**
   - Navigate to Register page
   - Create an account

2. **Test CRUD operations**
   - Create categories
   - Add materials
   - Edit and delete items

3. **Test authentication**
   - Log out
   - Try accessing protected pages
   - Log back in

---

## Common Issues and Solutions

### Issue 1: LocalDB not supported (macOS/Linux)

**Solution**: Use SQLite instead

```bash
# Remove SQL Server package
dotnet remove package Microsoft.EntityFrameworkCore.SqlServer

# Add SQLite package
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

Update `Program.cs`:
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
```

Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=MaterialManagement.db"
  }
}
```

### Issue 2: Migration errors

```bash
# Delete migrations folder
rm -rf Migrations

# Recreate migration
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Issue 3: Port already in use

Change port in `Properties/launchSettings.json` or use:
```bash
dotnet run --urls "http://localhost:5005"
```

---

## Learning Objectives Checklist

By completing this tutorial, you should understand:

- [ ] ASP.NET Core MVC architecture
- [ ] Entity Framework Core and database relationships
- [ ] Identity framework for authentication
- [ ] CRUD operations in controllers
- [ ] Database migrations
- [ ] Dependency injection
- [ ] Docker containerization
- [ ] Git version control
- [ ] GitHub repository management

---

## Next Steps

1. **Add Views**: Create Razor views for all controller actions
2. **Add Validation**: Implement data annotations and client-side validation
3. **Improve UI**: Add Bootstrap styling and responsive design
4. **Add Search**: Implement filtering and search functionality
5. **Add Pagination**: Handle large datasets efficiently
6. **Deploy**: Deploy to Azure, AWS, or other cloud platforms
7. **Add API**: Create REST API endpoints
8. **Add Testing**: Write unit and integration tests

---

## Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core)
- [ASP.NET Core Identity](https://docs.microsoft.com/aspnet/core/security/authentication/identity)
- [Docker Documentation](https://docs.docker.com/)
- [Git Documentation](https://git-scm.com/doc)
- [GitHub Guides](https://guides.github.com/)

---

## Assignment Ideas for Students

### Beginner
1. Add a new field to the Material model (e.g., SKU, ExpiryDate)
2. Create a new controller for Suppliers
3. Add custom validation to models

### Intermediate
1. Implement role-based authorization (Admin, Manager, User)
2. Add search and filter functionality
3. Create a dashboard with statistics
4. Add file upload for material images

### Advanced
1. Implement repository pattern
2. Add unit tests
3. Create a REST API
4. Implement pagination and sorting
5. Add logging with Serilog
6. Deploy to Azure or AWS

---

## Conclusion

Congratulations! You've built a complete ASP.NET Core application with authentication, database integration, and version control. This foundation can be extended to create more complex enterprise applications.

**Remember**: Practice makes perfect. Try modifying the code, breaking things, and fixing them to deepen your understanding.

Happy coding!
