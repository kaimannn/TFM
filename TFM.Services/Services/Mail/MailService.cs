using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace TFM.Services.Mail
{
    public interface IMailService
    {
        Task SendMail(MailMessage mail);
    }

    public class MailService : IMailService, IDisposable
    {
        private bool disposed = false;

        private readonly Data.Models.Configuration.AppSettings _config = null;

        private readonly SmtpClient _client = null;
        private readonly SemaphoreSlim _locker = new SemaphoreSlim(1);

        public MailService(Data.Models.Configuration.AppSettings config)
        {
            _config = config;

            _client = new SmtpClient(config.Mail.Host, config.Mail.Port)
            {
                UseDefaultCredentials = false,
                EnableSsl = true,
                Credentials = new NetworkCredential(config.Mail.Username, config.Mail.Password)
            };
        }

        public async Task SendMail(MailMessage mail)
        {
            try
            {
                mail.From ??= new MailAddress(_config.Mail.Username, _config.Mail.DisplayName);
                mail.To.Add(_config.Mail.To);

                await _locker.WaitAsync();

                await _client.SendMailAsync(mail);
            }
            finally
            {
                _locker.Release();
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            _locker.Dispose();
            _client?.Dispose();

            disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
