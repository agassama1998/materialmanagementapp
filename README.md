Ahmadou-test01
test

test01 # Material Management Application

A comprehensive ASP.NET Core 8.0 MVC application for managing materials and categories with user authentication and role-based authorization.

## Features

- **User Authentication**: ASP.NET Core Identity with registration and login
- **Role-Based Authorization**: Admin, Manager, and User roles
- **CRUD Operations**: Full Create, Read, Update, Delete functionality
- **Entity Framework Core**: Code-first approach with migrations
- **Responsive Design**: Bootstrap-based UI
- **Docker Support**: Multi-stage Dockerfile for containerization
- **Database Seeding**: Automatic role and admin user creation

## Technologies Used

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: Entity Framework Core with SQL Server
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Razor Views, Bootstrap 5
- **Containerization**: Docker
- **Version Control**: Git

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- SQL Server or SQLite (for cross-platform)
- [Docker](https://www.docker.com/) (optional)
- [Git](https://git-scm.com/)

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/informatixs/materialmanagementapp.git
cd materialmanagementapp
```

### 2. Configure Database

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MaterialManagementDB;Trusted_Connection=True;"
  }
}
```

**For SQLite (cross-platform):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=MaterialManagement.db"
  }
}
```

### 3. Apply Migrations

```bash
dotnet ef database update
```

### 4. Run the Application

```bash
dotnet run
```

Navigate to: `https://localhost:5001`

## Default Credentials

After running the application for the first time:

- **Email**: admin@materialmanagement.com
- **Password**: Admin@123

## Project Structure

```
MaterialManagement/
├── Controllers/
│   ├── CategoriesController.cs
│   ├── HomeController.cs
│   └── MaterialsController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── SeedData.cs
├── Models/
│   ├── Category.cs
│   └── Material.cs
├── Views/
│   ├── Categories/
│   ├── Materials/
│   ├── Home/
│   └── Shared/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── lib/
├── appsettings.json
├── Program.cs
├── Dockerfile
└── .gitignore
```

## Docker Deployment

### Build Docker Image

```bash
docker build -t materialmanagement .
```

### Run Container

```bash
docker run -p 8080:8080 -p 8081:8081 materialmanagement
```

Access the app at: `http://localhost:8080`

## Database Schema

### Categories Table
- CategoryId (PK)
- Name
- Description

### Materials Table
- MaterialId (PK)
- Name
- Description
- Price
- Quantity
- CategoryId (FK)

### Relationships
- One Category can have many Materials (One-to-Many)

## Common Commands

### Entity Framework

```bash
# Create a new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove

# Drop database
dotnet ef database drop
```

### Run Application

```bash
# Standard run
dotnet run

# With hot reload
dotnet watch run

# Specific environment
dotnet run --environment Production
```

### Build and Publish

```bash
# Build project
dotnet build

# Publish for deployment
dotnet publish -c Release -o ./publish
```

## Development Workflow

1. **Create a new feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make changes and test**
   ```bash
   dotnet watch run
   ```

3. **Commit changes**
   ```bash
   git add .
   git commit -m "Add feature: description"
   ```

4. **Push to GitHub**
   ```bash
   git push origin feature/your-feature-name
   ```

5. **Create Pull Request** on GitHub

## Learning Resources

For a detailed step-by-step tutorial on building this application from scratch, see [TUTORIAL.md](TUTORIAL.md)

### What You'll Learn
- ASP.NET Core MVC fundamentals
- Entity Framework Core with Code First
- Identity framework implementation
- CRUD operations
- Database relationships
- Docker containerization
- Git version control

## Troubleshooting

### Issue: LocalDB not available (macOS/Linux)

**Solution**: Switch to SQLite

1. Remove SQL Server package:
   ```bash
   dotnet remove package Microsoft.EntityFrameworkCore.SqlServer
   ```

2. Add SQLite package:
   ```bash
   dotnet add package Microsoft.EntityFrameworkCore.Sqlite
   ```

3. Update `Program.cs`:
   ```csharp
   options.UseSqlite(connectionString)
   ```

### Issue: Port already in use

**Solution**: Change port
```bash
dotnet run --urls "http://localhost:5005"
```

### Issue: Migration errors

**Solution**: Reset migrations
```bash
dotnet ef database drop
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed for educational purposes.

## Authors

- Tutorial created for ASP.NET Core training

## Acknowledgments

- ASP.NET Core Team
- Entity Framework Core Team
- Bootstrap Team

## Support

For issues and questions:
- Check [TUTORIAL.md](TUTORIAL.md) for detailed instructions
- Review ASP.NET Core [documentation](https://docs.microsoft.com/aspnet/core)
- Open an issue in the GitHub repository

---

**Happy Coding!** 🚀
