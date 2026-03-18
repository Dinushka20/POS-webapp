Remove-Item -Recurse -Force .git
git init
git checkout -b main
Set-Content README.md "# POS-webapp"
git add .gitignore README.md
git commit -m "chore: initialize repository with README and gitignore

- Set up empty main branch as the default foundational baseline
- Added standard .NET gitignore to prevent bin/obj/ and sensitive files from being tracked"

# Pushing main first sets it as the default branch if the repository is empty
git remote add origin https://github.com/Dinushka20/POS-webapp.git
git push -u origin main --force

git checkout -b feature/backend-api

git add POS.Api/POS.Api.csproj POS.Api/Program.cs POS.Api/appsettings.* POS.Api/Dockerfile POS.Api/Properties/ POS.Api/Enums/
git commit -m "chore(setup): scaffold ASP.NET Core 8 Web API architecture

- Generated core POS.Api project targeting .NET 8.0
- Configured appsettings.json for JWT Authentication and SQL Server LocalDB
- Configured Program.cs pipeline with CORS, Hangfire, Swagger, and Identity
- Added multi-stage Dockerfile for containerized deployment
- Defined application-wide enums (UserRole, OrderStatus, etc.)"

git add POS.Api/Models/
git commit -m "feat(domain): design database entity models and core schema

- Created AppUser identity model mapped to Branch
- Implemented core domain entities: Product, Category, Branch, Customer
- Added foundational models for transactions: Order, OrderItem
- Engineered inventory tracking entities: Stock, StockAuditLog"

git add POS.Api/Data/ POS.Api/Migrations/
git commit -m "feat(database): configure AppDbContext and initial EF Core migration

- Inherited IdentityDbContext<AppUser> for built-in auth support
- Configured fluent API mappings (e.g., decimal(18,2) precision)
- Implemented cascade delete rules and unique indexes natively
- Seeded initial branches and default product categories
- Applied 'InitialCreate' EF Core migration snapshot"

git add POS.Api/DTOs/
git commit -m "feat(api): construct comprehensive Data Transfer Objects (DTOs)

- Added securely scoped DTOs for Auth (Login, Register, Tokens)
- Built request validation payloads (CreateProduct, CreateOrder, StockAdjust)
- Created sanitized response models preventing domain leakages
- Implemented DataAnnotations for automatic 400 Bad Request model state validation"

git add POS.Api/Repositories/
git commit -m "feat(data): implement Repository pattern for abstracted data access

- Created isolated interfaces (IProductRepository, ICustomerRepository, etc.)
- Implemented EF Core data access layers decoupling controllers from DbContext
- Added robust eager loading methodologies (.Include()) reducing N+1 query bugs
- Implemented comprehensive generic fetching, updating, and specialized lookup logic"

git add POS.Api/Services/
git commit -m "feat(services): develop core business logic and background services

- AuthService: Implemented secure symmetric JWT key token generation
- OrderService: Engineered localized EF Core transactional boundaries for complex sales processing, inventory deduction, and customer points calculation
- PdfService: Integrated QuestPDF for memory-efficient thermal receipts and grid-based daily sales reporting
- ReportService: Implemented localized analytics (daily sales, top products, hourly activity aggregates)"

git add POS.Api/Hubs/
git commit -m "feat(realtime): integrate SignalR for instantaneous client synchronization

- Created SalesHub intercepting real-time notifications
- Setup dynamically authenticated scoped connections using branchId JWT claims
- Engineered seamless WebSockets broadcasts for 'NewSale' and 'LowStock' alerts"

git add POS.Api/Controllers/
git commit -m "feat(controllers): expose strongly-typed RESTful API endpoints

- Mapped logical HTTP endpoints mapping tightly bounded Route templates
- Implemented rigorous [Authorize(Roles)] role-based access controls
- Delegated request handling and mapping purely to internal Services and Repositories
- Structured consistent unhandled error catch blocks emitting proper 201/204/400/500 stateless HTTP protocol codes"

git add .
git commit -m "chore(cleanup): finalize project files and testing documentation

- Consolidated loose controller logic and API testing guides
- Verified functional boundaries and integrated end-to-end API lifecycle flow"

git push -u origin feature/backend-api --force
