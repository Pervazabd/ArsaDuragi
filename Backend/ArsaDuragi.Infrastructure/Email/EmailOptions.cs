namespace ArsaDuragi.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>When false (default) e-mails are only written to the log. No SMTP server is required for development.</summary>
    public bool Enabled { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // user-secrets / env var, never in the repo
    public string FromAddress { get; set; } = "no-reply@arsaduragi.az";
    public string FromName { get; set; } = "ArsaDuragi";
}
