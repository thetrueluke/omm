using System.Runtime.InteropServices;
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

        public async Task WriteMail(Migrator.Common.Mailbox mailbox, IMail mail)
        {
            var filename = GetFilename(mailbox.Name);
            if (!File.Exists(filename))
            {
                throw new Exception($"Mailbox with name {mailbox.Name} doesn't exists.");
            }

            try
            {
                using var stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

                var fmailbox = await JsonSerializer.DeserializeAsync<Mailbox>(stream, JsonSerializerOptions) ?? throw new Exception($"Mailbox {mailbox.Name} corrupt.");
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

                stream.Position = 0;
                JsonSerializer.Serialize(stream, fmailbox, JsonSerializerOptions);
            }
            catch (IOException)
            {
                const int ERROR_SHARING_VIOLATION = 32;
                if (Marshal.GetLastWin32Error() == ERROR_SHARING_VIOLATION)
                {
                    Console.WriteLine("The process cannot access the file because it is being used by another process.");
                }
                throw;
            }

            //File.WriteAllText(filename, JsonSerializer.Serialize(fmailbox, JsonSerializerOptions));
        }

        private string GetFilename(string name)
        {
            return Path.Combine(Migration.PathBase, Name, name + ".json");
        }
    }
}
