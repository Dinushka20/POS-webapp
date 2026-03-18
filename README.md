# POS system Web API

A comprehensive, production-ready ASP.NET Core 8 Web API backend tailored for modern Point of Sale (POS) systems. This architecture handles robust inventory tracking, secure role-based JWT authentication, transactional order processing, real-time WebSocket synchronization, and dynamic PDF receipt generation natively.

## 🚀 Features

- **Robust Architecture**: Built natively on ASP.NET Core 8 with cleanly decoupled Repositories and Services.
- **Entity Framework Core**: Code-first entity modeling with comprehensive SQL Server mapping, cascading constraints, and data-integrity seeding.
- **Role-Based Security**: Fully integrated JWT Bearer Authentication gated by robust Identity Roles (*Admin, Manager, Cashier*).
- **Real-Time Synergy**: SignalR `SalesHub` pushes live 'LowStock' and 'NewSale' notifications instantly across branch WebSocket listeners.
- **Advanced Transactions**: Seamless atomic inventory processing and automated Customer Loyalty Point generation.
- **Background Jobs**: Hangfire configuration out-of-the-box for distributed, persistent background task execution safely relying on SQL Server.
- **Dynamic PDF Reporting**: QuestPDF integration natively drawing memory-efficient A4 Sales Reports and Thermal receipt layouts.
- **Dockerized Ready**: Ships with a fully pre-configured, modular multi-stage `Dockerfile`.

## 🛠️ Tech Stack

- **Framework**: .NET 8.0 ASP.NET Core Web API
- **Data Access**: Entity Framework Core 8 (SQL Server)
- **Identity & Security**: ASP.NET Core Identity & JWT Bearer
- **Real-Time Communication**: Microsoft.AspNetCore.SignalR
- **Background Processing**: Hangfire & Hangfire.SqlServer
- **PDF Generation**: QuestPDF (Community License)
- **API Documentation**: Swagger (Swashbuckle) natively configured for HTTP Bearer workflows.

## ⚙️ Local Development Setup

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express, LocalDB, or Developer)

### 1. Configure the Database
Inside `POS.Api/appsettings.json` and `POS.Api/appsettings.Development.json`, update the `DefaultConnection` string with your SQL Server instance:
```json
"ConnectionStrings": {
    "DefaultConnection": "Data Source=YOUR_INSTANCE;Initial Catalog=PosDbDev;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;"
}
```

### 2. Run the Application
The `Program.cs` file natively intercepts execution and enforces `context.Database.Migrate()`. The database, tables, and seeded records are constructed fully automatically!
```bash
cd POS.Api
dotnet run
```

### 3. Test the Endpoints
Visit `http://localhost:5277/swagger` to explore the fully functional Swagger UI. Register an account natively, and paste the generated JWT token securely inside the Authorization shield overlay to instantly test authenticated Endpoints.

## 🏗️ Structure Overview
- **/Controllers**: Secured endpoints mapping HTTP Verbs to Application logic.
- **/Models & /DTOs**: Highly insulated Request/Response bodies cleanly decoupled from core Domain Entities preventing data leakage.
- **/Services**: Encapsulated cross-domain transaction boundaries resolving massive state alterations locally.
- **/Repositories**: Scalable database interaction contexts natively eliminating scattered LINQ logic.

---
*Built dynamically and cleanly formatted for modern software engineering standards.*
