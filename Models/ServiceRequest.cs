namespace StateServices.Api.Models;

public enum RequestStatus
{
    Open,
    InProgress,
    Resolved
}

public sealed record ServiceRequest(
    string Id,
    string Resident,
    string Service,
    string County,
    DateOnly Submitted,
    RequestStatus Status);

public sealed record ServiceRequestDto(
    string Id,
    string Resident,
    string Service,
    string County,
    DateOnly Submitted,
    RequestStatus Status);

public sealed record CreateServiceRequestDto(
    string Resident,
    string Service,
    string County);

public sealed record UpdateRequestStatusDto(RequestStatus Status);