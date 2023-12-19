using System.Text.Json;
using Migrator.Common;

namespace AlmostMailProvider
{
    public class AlmostMailProvider : IMailProvider
    {
        public string Name => GetType().Name.Replace("Provider", "");

        public async Task CreateMailbox(Mailbox mailbox)
        {
            string filename = GetFilename(mailbox.Name);
            if (File.Exists(filename))
            {
                throw new ArgumentException($"Mailbox with name already exists.");
            }
            await File.WriteAllTextAsync(filename, JsonSerializer.Serialize(new Mail()
            {
                MailboxQuota = mailbox.Quota,
                Password = mailbox.Password
            }, new JsonSerializerOptions()
            {
                WriteIndented = true
            }));
        }

        public Task<Mailbox> GetMailbox(string username, string password)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IMail>> GetMails(Mailbox mailbox)
        {
            throw new NotImplementedException();
        }

        public Task WriteMail(Mailbox mailbox, IMail mail)
        {
            throw new NotImplementedException();
        }

        private string GetFilename(string name)
        {
            return Path.Combine(Migration.PathBase, Name, name + ".json");
        }
    }
}
