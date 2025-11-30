namespace ArlaNatureConnect.Core.DTOs;

// Purpose: Represents the farm summary shown in lists (name, address, owner, status).
// Notes: Used by both services and WinUI to avoid leaking entity internals and to carry computed strings.
public sealed class FarmAssignmentOverviewDto
{
    public Guid FarmId { get; init; }
    public string FarmName { get; init; } = string.Empty;
    public string Cvr { get; init; } = string.Empty;
    public string OwnerFirstName { get; init; } = string.Empty;
    public string OwnerLastName { get; init; } = string.Empty;
    public string OwnerEmail { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public bool HasActiveCase { get; init; }
    public string? AssignedConsultantFirstName { get; init; }
    public string? AssignedConsultantLastName { get; init; }
    public Guid? AssignedConsultantId { get; init; }
    public string? Priority { get; init; }
    public string? Notes { get; init; }

    public string OwnerName => $"{OwnerFirstName} {OwnerLastName}".Trim();

    public string AddressLine => $"{Street}, {PostalCode} {City}".Trim().Trim(',', ' ');

    public string StatusLabel => HasActiveCase ? "Tilføjet" : "Ikke tilføjet";
    public string AssignedConsultantName => $"{AssignedConsultantFirstName} {AssignedConsultantLastName}".Trim();
}

