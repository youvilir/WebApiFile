namespace WebApiFile.Services.Email
{
    public interface IEmailService
    {
        public Task SendEmailAsync(string message);
    }
}
