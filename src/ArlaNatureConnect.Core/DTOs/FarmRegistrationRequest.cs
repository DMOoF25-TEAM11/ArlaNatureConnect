namespace ArlaNatureConnect.Core.DTOs;

// Purpose: Describes the information required to create or update a farm.
// Notes: Used by the NatureCheckCaseService to orchestrate farm, address and owner persistence in one call.
public sealed class FarmRegistrationRequest
{
    public Guid? FarmId { get; init; }
    public string FarmName { get; init; } = string.Empty;
    public string Cvr { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = "Danmark";
    public string OwnerFirstName { get; init; } = string.Empty;
    public string OwnerLastName { get; init; } = string.Empty;
    public string OwnerEmail { get; init; } = string.Empty;
    public bool OwnerIsActive { get; init; } = true;
}


