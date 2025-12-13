namespace ArlaNatureConnect.Core.Abstract;

public interface IEmailService
{
    Task SendNatureCheckCreatedEmailAsync(
        string toEmail,
        string consultantName,
        string farmName,
        string farmAddress,
        string farmCvr,
        DateTime dateTime,
        Guid natureCheckId,
        CancellationToken cancellationToken = default);
}
