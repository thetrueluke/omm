using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Migrator.Common;

namespace AlmostMailProvider
{
    public class AlmostMailProvider : IMailProvider
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        public string Name => GetType().Name.Replace("Provider", "");

        public async Task<Migrator.Common.Mailbox> GetMailbox(string username, string password)
        {
            var filename = GetFilename(username);
            var fileContents = await File.ReadAllTextAsync(filename);
            var mailbox = GetMailbox(fileContents);
            if (mailbox is not null)
            {
                if (mailbox.Password == password)
                {
                    return new Migrator.Common.Mailbox()
                    {
                        Name = username,
                        Password = password,
                        Quota = mailbox.MailboxQuota
                    };
                }
                else
                {
                    throw new UnauthorizedAccessException($"Password incorrect for source mailbox of name {username}.");
                }
            }
            else
            {
                throw new KeyNotFoundException($"Source mailbox of name {username} not found.");
            }
        }

        public async Task<IEnumerable<IMail>> GetMails(Migrator.Common.Mailbox mailbox)
        {
            var filename = GetFilename(mailbox.Name);
            var fileContents = await File.ReadAllTextAsync(filename);
            var fmailbox = GetMailbox(fileContents);
            return fmailbox?.Mails ?? Enumerable.Empty<IMail>();
        }

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

        public async Task WriteMail(Migrator.Common.Mailbox mailbox, IMail mail)
        {
            await Task.Run(() => //The reason for such a strange solution is that mutex need to be released by the same thread that created it.
            {
                var filename = GetFilename(mailbox.Name);
                if (!File.Exists(filename))
                {
                    throw new Exception($"Mailbox with name {mailbox.Name} doesn't exists.");
                }

                var mutex = new Mutex(false, $"{GetType().Name}|{filename.Replace(@"\", "/")}"); //'\' character is reserved for mutex' path prefix. Also, we specify false in initiallyOwned, as we acquire it with WaitOne in try block cause it can throw (e.g.) abondoned exception.
                try
                {
                    mutex.WaitOne();
                    var fileContents = File.ReadAllText(filename);
                    var fmailbox = GetMailbox(fileContents) ?? throw new Exception($"Mailbox {mailbox.Name} corrupt.");
                    if (fmailbox.Password != mailbox.Password)
                    {
                        throw new Exception($"Password incorrect for source mailbox of name {mailbox.Name}.");
                    }
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
                }
                //catch (Exception ex)
                //{
                //    throw;
                //}
                finally
                {
                    mutex.ReleaseMutex();
                }
            });
        }

        private string GetFilename(string name)
        {
            return Path.Combine(Migration.PathBase, Name, name + ".json");
        }

        private static Mailbox? GetMailbox(string fileContents)
        {
            return JsonSerializer.Deserialize<Mailbox>(fileContents, JsonSerializerOptions);
        }
    }
}
