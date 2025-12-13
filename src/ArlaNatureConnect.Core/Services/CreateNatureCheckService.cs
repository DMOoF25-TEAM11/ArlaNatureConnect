using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;

namespace ArlaNatureConnect.Core.Services;

public class CreateNatureCheckService : ICreateNatureCheck
{
    private readonly IFarmRepository _farmRepository;
    private readonly IPersonRepository _personRepository;
    private readonly INatureCheckCaseRepository _natureCheckCaseRepository;
    private readonly ICreateNatureCheckRepository _createNatureCheckRepository;
    private readonly IEmailService _emailService;

    public CreateNatureCheckService(
        IFarmRepository farmRepository,
        IPersonRepository personRepository,
        INatureCheckCaseRepository natureCheckCaseRepository,
        ICreateNatureCheckRepository createNatureCheckRepository,
        IEmailService emailService)
    {
        _farmRepository = farmRepository ?? throw new ArgumentNullException(nameof(farmRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _natureCheckCaseRepository = natureCheckCaseRepository ?? throw new ArgumentNullException(nameof(natureCheckCaseRepository));
        _createNatureCheckRepository = createNatureCheckRepository ?? throw new ArgumentNullException(nameof(createNatureCheckRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    public async Task<List<Farm>> GetFarmsAsync()
    {
        var farms = await _farmRepository.GetAllAsync();
        return farms.OrderBy(f => f.Name).ToList();
    }

    public async Task<List<Person>> GetPersonsAsync()
    {
        var persons = await _personRepository.GetAllAsync();
        return persons.OrderBy(p => p.LastName).ToList();
    }

    public async Task<List<NatureCheckCase>> GetNatureChecksAsync()
    {
        var checks = await _natureCheckCaseRepository.GetAllAsync();
        return checks.OrderByDescending(n => n.CreatedAt).ToList();
    }

    public async Task<Guid> CreateNatureCheckAsync(
    CreateNatureCheck request,
    CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        // 1) Save to DB (via stored procedure)
        var id = await _createNatureCheckRepository.CreateNatureCheckAsync(request, cancellationToken);

        // 2) Load extra data for email (consultant email + farm info)
        var consultant = await _personRepository.GetByIdAsync(request.PersonId);
        var farm = await _farmRepository.GetByIdAsync(request.FarmId);

        var toEmail = consultant?.Email;
        var consultantName = consultant is null
            ? $"{request.ConsultantFirstName} {request.ConsultantLastName}"
            : $"{consultant.FirstName} {consultant.LastName}";

        var farmName = farm?.Name ?? request.FarmName;
        var farmAddress = request.FarmAddress;
        var farmCvr = (farm?.CVR ?? request.FarmCVR.ToString());

        // 3) Send email (do NOT fail the operation if email fails)
        try
        {
            await _emailService.SendNatureCheckCreatedEmailAsync(
                toEmail: toEmail ?? string.Empty,
                consultantName: consultantName,
                farmName: farmName,
                farmAddress: farmAddress,
                farmCvr: farmCvr,
                dateTime: request.DateTime,
                natureCheckId: id,
                cancellationToken: cancellationToken);
        }
        catch
        {
        }
        return id;
    }
}
