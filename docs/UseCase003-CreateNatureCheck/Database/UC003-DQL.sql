SELECT
    nc.Id           AS NatureCheckId,
    nc.FarmId,
    f.Name          AS FarmName,
    f.CVR,
    f.AddressId,
    nc.PersonId,
    p.FirstName     AS ConsultantFirstName,
    p.LastName      AS ConsultantLastName,
    nc.Date         AS NatureCheckDate

FROM dbo.NatureCheck AS nc
INNER JOIN dbo.Persons AS p
    ON nc.PersonId = p.Id
INNER JOIN dbo.Farms AS f
    ON nc.FarmId = f.Id;