using System.Net;
using System.Net.Mail;

using ArlaNatureConnect.Core.Abstract;

namespace ArlaNatureConnect.Infrastructure.ExternalServices;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;

    public SmtpEmailService(SmtpSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task SendNatureCheckCreatedEmailAsync(
        string toEmail,
        string consultantName,
        string farmName,
        string farmAddress,
        string farmCvr,
        DateTime dateTime,
        Guid natureCheckId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            return;

        var subject = "Nyt naturtjek oprettet";
        var body = $@"
Hej {consultantName},

Der er lige blevet oprettet et nyt naturtjek.

Detaljer:
- Naturtjek ID: {natureCheckId}
- Gård: {farmName}
- CVR: {farmCvr}
- Adresse: {farmAddress}
- Dato/Tid: {dateTime:dd-MM-yyyy HH:mm}

Venlig hilsen
Arla NatureConnect
";

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(toEmail);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        
        await client.SendMailAsync(message);
    }
}
