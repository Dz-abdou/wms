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

4. Run the API with `dotnet run --project src/Warehouse.Api`.
5. Copy `frontend/warehouse-web/.env.example` to `frontend/warehouse-web/.env` when the API uses a non-default URL or port.
6. Run the frontend with `npm run dev --prefix frontend/warehouse-web`.

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