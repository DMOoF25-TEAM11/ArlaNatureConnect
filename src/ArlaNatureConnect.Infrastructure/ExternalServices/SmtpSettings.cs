namespace ArlaNatureConnect.Infrastructure.ExternalServices;

public class SmtpSettings
{
    public string Host { get; init; } = default!;
    public int Port { get; init; }
    public bool EnableSsl { get; init; }
    public string Username { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string FromEmail { get; init; } = default!;
    public string FromName { get; init; } = "Arla NatureConnect";
}
