using Microsoft.OpenApi.Models;
using StateServices.Api.Models;
using StateServices.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "State Services API",
        Version = "v1",
        Description = "Sample ASP.NET Core REST API for a fictional public service request dashboard. Uses in-memory demo data only."
    });
});
builder.Services.AddSingleton<ServiceRequestRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "State Services API v1");
    options.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Redirect("/swagger"))
    .WithName("SwaggerHome")
    .ExcludeFromDescription();

app.MapGet("/api/requests", (string? search, RequestStatus? status, ServiceRequestRepository repository) =>
    {
        var results = repository.GetAll(search, status);
        return Results.Ok(results);
    })
    .WithName("GetServiceRequests")
    .WithSummary("Get service requests")
    .WithDescription("Optionally filter by a search term or status.")
    .Produces<IReadOnlyList<ServiceRequestDto>>(StatusCodes.Status200OK);

app.MapGet("/api/requests/{id}", (string id, ServiceRequestRepository repository) =>
    {
        var request = repository.GetById(id);
        return request is null ? Results.NotFound(new { message = $"Request '{id}' was not found." }) : Results.Ok(request);
    })
    .WithName("GetServiceRequestById")
    .WithSummary("Get a service request by ID")
    .Produces<ServiceRequestDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

app.MapPost("/api/requests", (CreateServiceRequestDto input, ServiceRequestRepository repository) =>
    {
        var errors = Validate(input);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var created = repository.Create(input);
        return Results.Created($"/api/requests/{created.Id}", created);
    })
    .WithName("CreateServiceRequest")
    .WithSummary("Create a service request")
    .Produces<ServiceRequestDto>(StatusCodes.Status201Created)
    .ProducesValidationProblem();

app.MapPatch("/api/requests/{id}/status", (string id, UpdateRequestStatusDto input, ServiceRequestRepository repository) =>
    {
        var updated = repository.UpdateStatus(id, input.Status);
        return updated is null
            ? Results.NotFound(new { message = $"Request '{id}' was not found." })
            : Results.Ok(updated);
    })
    .WithName("UpdateServiceRequestStatus")
    .WithSummary("Update a request status")
    .Produces<ServiceRequestDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .ProducesValidationProblem();

app.MapDelete("/api/requests/{id}", (string id, ServiceRequestRepository repository) =>
    {
        return repository.Delete(id)
            ? Results.NoContent()
            : Results.NotFound(new { message = $"Request '{id}' was not found." });
    })
    .WithName("DeleteServiceRequest")
    .WithSummary("Delete a service request")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound);

app.Run();

static Dictionary<string, string[]> Validate(CreateServiceRequestDto input)
{
    var errors = new Dictionary<string, string[]>();
    if (string.IsNullOrWhiteSpace(input.Resident) || input.Resident.Trim().Length > 100)
        errors["resident"] = ["Resident is required and must be 100 characters or fewer."];
    if (string.IsNullOrWhiteSpace(input.Service) || input.Service.Trim().Length > 120)
        errors["service"] = ["Service is required and must be 120 characters or fewer."];
    if (string.IsNullOrWhiteSpace(input.County) || input.County.Trim().Length > 80)
        errors["county"] = ["County is required and must be 80 characters or fewer."];
    return errors;
}