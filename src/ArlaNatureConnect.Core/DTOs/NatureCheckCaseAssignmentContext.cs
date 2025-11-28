using ArlaNatureConnect.Domain.Entities;

namespace ArlaNatureConnect.Core.DTOs;

// Purpose: Bundles farm overviews and consultants so UC002B viewmodels can load everything with one call.
// Notes: Keeps service signatures small and enables future caching if needed.
public sealed class NatureCheckCaseAssignmentContext
{
    public NatureCheckCaseAssignmentContext(
        IReadOnlyList<FarmAssignmentOverviewDto> farms,
        IReadOnlyList<Person> consultants)
    {
        Farms = farms;
        Consultants = consultants;
    }

    public IReadOnlyList<FarmAssignmentOverviewDto> Farms { get; }
    public IReadOnlyList<Person> Consultants { get; }
}


