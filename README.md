# Warehouse Management System

A portfolio warehouse management system built with ASP.NET Core, PostgreSQL, React, and TypeScript.

## Local setup

1. Copy `.env.example` to `.env` if you need non-default PostgreSQL settings.
2. Start PostgreSQL with `docker compose up -d`.
3. Generate and review migrations manually after model changes. Codex does not generate or edit migrations:

   ```text
   dotnet tool update --global dotnet-ef --version "10.0.*"
   dotnet ef migrations add AddProducts --project src/Warehouse.Infrastructure --startup-project src/Warehouse.Api
   dotnet ef database update --project src/Warehouse.Infrastructure --startup-project src/Warehouse.Api
   ```

4. Configure development-only authentication secrets. Never put these values in
   appsettings files, .env, or source control:

   ```text
   dotnet user-secrets init --project src/Warehouse.Api
   dotnet user-secrets set "Jwt:Issuer" "warehouse-api" --project src/Warehouse.Api
   dotnet user-secrets set "Jwt:Audience" "warehouse-web" --project src/Warehouse.Api
   dotnet user-secrets set "Jwt:SigningKey" "<a random value of at least 32 characters>" --project src/Warehouse.Api
   dotnet user-secrets set "Jwt:AccessTokenMinutes" "15" --project src/Warehouse.Api
   dotnet user-secrets set "DevelopmentAdmin:Email" "admin@example.test" --project src/Warehouse.Api
   dotnet user-secrets set "DevelopmentAdmin:Password" "WarehouseDev123!" --project src/Warehouse.Api
   ```

   Equivalent environment-variable names are:

   ```text
   Jwt__Issuer
   Jwt__Audience
   Jwt__SigningKey
   Jwt__AccessTokenMinutes
   DevelopmentAdmin__Email
   DevelopmentAdmin__Password
   ```

   The development administrator is created only when both bootstrap values are
   configured. Use a unique, strong password outside local development.

5. Run the API with `dotnet run --project src/Warehouse.Api`.
6. Copy `frontend/warehouse-web/.env.example` to `frontend/warehouse-web/.env` when the API uses a non-default URL or port.
7. Run the frontend with `npm run dev --prefix frontend/warehouse-web`.

The API exposes `/health/live`, `/health/ready`, Product endpoints under `/api/products`, and Swagger UI in the Development environment.

## Verification

Run the backend tests (including PostgreSQL Testcontainers integration tests after the migration exists):

```text
dotnet restore Warehouse.sln
dotnet test Warehouse.sln --no-restore
```

Run the frontend checks:

```text
npm ci --prefix frontend/warehouse-web
npm run lint --prefix frontend/warehouse-web
npm run test:run --prefix frontend/warehouse-web
npm run build --prefix frontend/warehouse-web
```