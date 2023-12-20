using System.Security.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Migrator.Common;

namespace MerelyMailProvider
{
    public class MerelyMailProvider : IMailProvider
    {
        public string Name => GetType().Name.Replace("Provider", "");

        public Task CreateMailbox(Mailbox mailbox)
        {
            throw new NotImplementedException();
        }

        public async Task<Mailbox> GetMailbox(string username, string password)
        {
            using var context = new Models.MerelyMailContext();
            var mailbox = (await GetDbMailbox(context, username, password)) ?? throw new KeyNotFoundException($"Mailbox with name {username} not found."); //ToDo: Add throwing on wrong password only.
            return new Mailbox()
            {
                Name = mailbox.Email,
                Password = password,
                Quota = mailbox.Quota,
            };
        }

        public async Task<IEnumerable<IMail>> GetMails(Mailbox mailbox)
        {
            using var context = new Models.MerelyMailContext();
            var dbMailbox = await GetDbMailbox(context, mailbox.Name, mailbox.Password);
            if (dbMailbox is null)
            {
                return Enumerable.Empty<IMail>();
            }

            var dbMails = await context.Mails.Where(m => m.MailboxId == dbMailbox.Id).Join(context.Folders, m => m.FolderId, f => f.Id, (mail, folder) => new { mail, folder }).ToListAsync();
            //var dbMails = await context.Mails.Where(m => m.MailboxId == dbMailbox.Id).ToListAsync();
            var mails = dbMails.Select(m => new MerelyMail()
            {
                Body = m.mail.Body,
                Receiver = m.mail.To,
                Sender = m.mail.From,
                Size = m.mail.Size,
                Subject = m.mail.Subject,
                Folder = m.folder.Name
            });
            return mails;
        }

        public Task WriteMail(Mailbox mailbox, IMail mail)
        {
            throw new NotImplementedException();
        }

        private static async Task<Models.Mailbox?> GetDbMailbox(Models.MerelyMailContext context, string username, string password)
        {
            return await context.Mailboxes.FirstOrDefaultAsync(m => m.Email == username && m.Password == password);
        }
    }
}
