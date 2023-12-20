using System.Text.Json;
using Migrator.Common;

namespace AlmostMailProvider
{
    public class AlmostMailProvider : IMailProvider
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = true
        };

        public string Name => GetType().Name.Replace("Provider", "");

        public async Task CreateMailbox(Migrator.Common.Mailbox mailbox)
        {
            var filename = GetFilename(mailbox.Name);
            if (File.Exists(filename))
            {
                throw new ArgumentException($"Mailbox with name {mailbox.Name} already exists.");
            }
            await File.WriteAllTextAsync(filename, JsonSerializer.Serialize(new Mailbox()
            {
                MailboxQuota = mailbox.Quota,
                Password = mailbox.Password
            }, JsonSerializerOptions));
        }

        public Task<Migrator.Common.Mailbox> GetMailbox(string username, string password)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IMail>> GetMails(Migrator.Common.Mailbox mailbox)
        {
            throw new NotImplementedException();
        }

        private readonly object writeMailLock = new();
        public Task WriteMail(Migrator.Common.Mailbox mailbox, IMail mail)
        {
            lock (writeMailLock)
            {
                var filename = GetFilename(mailbox.Name);
                if (!File.Exists(filename))
                {
                    throw new Exception($"Mailbox with name {mailbox.Name} doesn't exists.");
                }

                var fmailbox = JsonSerializer.Deserialize<Mailbox>(File.ReadAllText(filename), JsonSerializerOptions) ?? throw new Exception($"Mailbox {mailbox.Name} corrupt.");
                fmailbox.Mails ??= [];
                fmailbox.Mails.Add(new AlmostMail()
                {
                    Receiver = mail.Receiver,
                    Sender = mail.Sender,
                    Body = mail.Body,
                    Folder = mail.Folder,
                    Size = mail.Size,
                    Subject = mail.Subject
                });
                fmailbox.MailboxSize = (double)fmailbox.Mails.Sum(m => m.Size) / 1024 / 1024 / 1024;

                File.WriteAllText(filename, JsonSerializer.Serialize(fmailbox, JsonSerializerOptions));
                return Task.CompletedTask;
            }
        }

        private string GetFilename(string name)
        {
            return Path.Combine(Migration.PathBase, Name, name + ".json");
        }
    }
}
