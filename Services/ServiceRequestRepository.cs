using StateServices.Api.Models;

namespace StateServices.Api.Services;

/// <summary>
/// In-memory repository for portfolio demonstration only.
/// Data resets when the application restarts; replace with EF Core and a database for production.
/// </summary>
public sealed class ServiceRequestRepository
{
    private readonly object _gate = new();
    private readonly List<ServiceRequest> _requests =
    [
        new("IA-1048", "Jordan Lee", "Property tax question", "Polk", new DateOnly(2026, 10, 24), RequestStatus.Open),
        new("IA-1047", "Morgan Davis", "License renewal", "Linn", new DateOnly(2026, 10, 23), RequestStatus.InProgress),
        new("IA-1046", "Taylor Smith", "Public records request", "Johnson", new DateOnly(2026, 10, 22), RequestStatus.Resolved),
        new("IA-1045", "Casey Patel", "Benefits application", "Story", new DateOnly(2026, 10, 21), RequestStatus.Open),
        new("IA-1044", "Alex Johnson", "Business registration", "Black Hawk", new DateOnly(2026, 10, 20), RequestStatus.InProgress),
        new("IA-1043", "Riley Chen", "Property tax question", "Polk", new DateOnly(2026, 10, 19), RequestStatus.Resolved)
    ];

    public IReadOnlyList<ServiceRequestDto> GetAll(string? search, RequestStatus? status)
    {
        lock (_gate)
        {
            var query = _requests.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(r =>
                    r.Id.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    r.Resident.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    r.Service.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    r.County.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            return query.OrderByDescending(r => r.Submitted)
                .Select(ToDto)
                .ToArray();
        }
    }

    public ServiceRequestDto? GetById(string id)
    {
        lock (_gate)
        {
            var request = _requests.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return request is null ? null : ToDto(request);
        }
    }

    public ServiceRequestDto Create(CreateServiceRequestDto input)
    {
        lock (_gate)
        {
            var nextNumber = _requests
                .Select(r => int.TryParse(r.Id.Replace("IA-", ""), out var n) ? n : 1000)
                .DefaultIfEmpty(1048)
                .Max() + 1;
            var request = new ServiceRequest(
                $"IA-{nextNumber}",
                input.Resident.Trim(),
                input.Service.Trim(),
                input.County.Trim(),
                DateOnly.FromDateTime(DateTime.UtcNow),
                RequestStatus.Open);
            _requests.Add(request);
            return ToDto(request);
        }
    }

    public ServiceRequestDto? UpdateStatus(string id, RequestStatus status)
    {
        lock (_gate)
        {
            var index = _requests.FindIndex(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (index < 0) return null;

            var current = _requests[index];
            var updated = current with { Status = status };
            _requests[index] = updated;
            return ToDto(updated);
        }
    }

    public bool Delete(string id)
    {
        lock (_gate)
        {
            var index = _requests.FindIndex(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (index < 0) return false;
            _requests.RemoveAt(index);
            return true;
        }
    }

    private static ServiceRequestDto ToDto(ServiceRequest r) =>
        new(r.Id, r.Resident, r.Service, r.County, r.Submitted, r.Status);
}