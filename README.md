# State Services API (ASP.NET Core + Swagger)

A portfolio REST API for the fictional State Services Management Dashboard. It demonstrates ASP.NET Core Minimal APIs, C#, typed DTOs, basic input validation, dependency injection, an in-memory repository, and interactive OpenAPI documentation with Swagger UI.

> **Demo only:** Not affiliated with the State of Iowa and not connected to any government system. All records are fictional. This starter uses in-memory storage, has no authentication, and is not production-ready.

## Requirements

- .NET 8 SDK
- Internet access for the first restore of the Swashbuckle NuGet package

## Run locally

From this directory:

```bash
dotnet restore
dotnet run
```

Open Swagger UI at `http://localhost:5080/swagger` (or use the HTTPS URL printed by `dotnet run`).

## Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/requests` | List requests; optional `search` and `status` query parameters |
| GET | `/api/requests/{id}` | Get a request by ID |
| POST | `/api/requests` | Create a request |
| PATCH | `/api/requests/{id}/status` | Update request status |
| DELETE | `/api/requests/{id}` | Delete a request |

Status enum values are `Open`, `InProgress`, and `Resolved`.

### Example POST body

```json
{
  "resident": "Jamie Example",
  "service": "Permit question",
  "county": "Polk"
}
```

### Example PATCH body

```json
{
  "status": "Resolved"
}
```

## Architecture

- `Program.cs`: endpoint registration, Swagger/OpenAPI setup, basic validation
- `Models/ServiceRequest.cs`: request domain model and API DTOs
- `Services/ServiceRequestRepository.cs`: singleton in-memory repository for the demo

The repository is registered through dependency injection. The singleton is safe for this simple demo because its shared list operations are guarded by a lock.

## Connect the Angular portfolio

1. Run this API on your development machine.
2. In Angular, create an injectable service using `HttpClient` to call `http://localhost:5080/api/requests`.
3. Configure CORS for the Angular development origin before making browser requests from a separate port.
4. Replace the Angular sample array with the API response and wire the Resolve action to `PATCH /api/requests/{id}/status`.

Do not enable permissive CORS or expose this unauthenticated demo publicly as a production service.

## Before publishing

- Run `dotnet restore` and `dotnet build`.
- Try each endpoint in Swagger UI.
- Consider adding xUnit tests and an EF Core-backed persistence layer.
- Never commit credentials, connection strings containing secrets, or employer-owned code.
