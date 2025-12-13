using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Domain.Enums;
using ArlaNatureConnect.Infrastructure.Persistence;
using Microsoft.Data.SqlClient; 
using System.Data;
using Microsoft.EntityFrameworkCore;

using System.Data;

namespace ArlaNatureConnect.Infrastructure.Repositories;

public class CreateNatureCheckRepository : ICreateNatureCheckRepository
{
    private readonly AppDbContext _db;

    public CreateNatureCheckRepository(AppDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    

public async Task<Guid> CreateNatureCheckAsync(
    CreateNatureCheck request,
    CancellationToken cancellationToken = default)
{
    if (request is null)
        throw new ArgumentNullException(nameof(request));

    // If NatureCheckId is empty, let the proc generate a new one
    var idParam = new SqlParameter("@NatureCheckId", SqlDbType.UniqueIdentifier)
    {
        Direction = ParameterDirection.InputOutput,
        Value = request.NatureCheckId == Guid.Empty
            ? (object)DBNull.Value
            : request.NatureCheckId
    };

    var farmIdParam = new SqlParameter("@FarmId", SqlDbType.UniqueIdentifier)
    {
        Value = request.FarmId
    };

    var personIdParam = new SqlParameter("@PersonId", SqlDbType.UniqueIdentifier)
    {
        Value = request.PersonId
    };

    var farmNameParam = new SqlParameter("@FarmName", SqlDbType.NVarChar, 200)
    {
        Value = (object?)request.FarmName ?? DBNull.Value
    };

    var farmCvrParam = new SqlParameter("@FarmCVR", SqlDbType.NVarChar, 50)
    {
        Value = request.FarmCVR.ToString()
    };

    var farmAddressParam = new SqlParameter("@FarmAddress", SqlDbType.NVarChar, 250)
    {
        Value = string.IsNullOrWhiteSpace(request.FarmAddress)
        ? (object)DBNull.Value
        : request.FarmAddress
    };


        var consultantFirstNameParam = new SqlParameter("@ConsultantFirstName", SqlDbType.NVarChar, 100)
    {
        Value = (object?)request.ConsultantFirstName ?? DBNull.Value
    };

    var consultantLastNameParam = new SqlParameter("@ConsultantLastName", SqlDbType.NVarChar, 100)
    {
        Value = (object?)request.ConsultantLastName ?? DBNull.Value
    };

    var dateParam = new SqlParameter("@DateTime", SqlDbType.DateTime2)
    {
        Value = request.DateTime
    };

    await _db.Database.ExecuteSqlRawAsync(
        @"EXEC dbo.uspCreateNatureCheck 
              @NatureCheckId OUTPUT,
              @FarmId,
              @PersonId,
              @FarmName,
              @FarmCVR,
              @FarmAddress,
              @ConsultantFirstName,
              @ConsultantLastName,
              @DateTime",
        idParam,
        farmIdParam,
        personIdParam,
        farmNameParam,
        farmCvrParam,
        farmAddressParam,
        consultantFirstNameParam,
        consultantLastNameParam,
        dateParam);

    // The proc sets @NatureCheckId if it was NULL
    return (Guid)idParam.Value;
}

public async Task<CreateNatureCheck?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.NatureCheckCases
            .Include(n => n.Farm)
            .Include(n => n.Consultant)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

        if (entity is null)
            return null;

        return new CreateNatureCheck
        {
            NatureCheckId = entity.Id,
            FarmId = entity.FarmId,
            PersonId = entity.ConsultantId,
            FarmName = entity.Farm.Name,
            FarmCVR = int.TryParse(entity.Farm.CVR, out var cvr) ? cvr : 0,
            FarmAddress = entity.Farm.Address != null
                ? $"{entity.Farm.Address.Street}, {entity.Farm.Address.PostalCode} {entity.Farm.Address.City}"
                : string.Empty,
            ConsultantFirstName = entity.Consultant.FirstName,
            ConsultantLastName = entity.Consultant.LastName,
            DateTime = entity.AssignedAt?.UtcDateTime ?? entity.CreatedAt.UtcDateTime
        };
    }
}
