using System.Security.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Migrator.Common;

namespace MerelyMailProvider
{
    public class MerelyMailProvider : IMailProvider
    {
        public string Name => GetType().Name.Replace("Provider", "");

        public async Task<Mailbox> GetMailbox(string username, string password)
        {
            using var context = new Models.MerelyMailContext();
            var mailbox = (await GetDbMailbox(context, username, password)) ?? throw new KeyNotFoundException($"Source mailbox of name {username} not found or password doesn't match."); //ToDo: Add throwing on wrong password only.
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

        public async Task CreateMailbox(Mailbox mailbox)
        {
            try
            {
                using var context = new Models.MerelyMailContext();
                if (context.Mailboxes.Where(m => m.Email == mailbox.Name).Any())
                {
                    throw new Exception($"Mailbox with name {mailbox.Name} already exists.");
                }
                var id = (await context.Mailboxes.MaxAsync(m => m.Id)) + 1;
                await context.Mailboxes.AddAsync(new Models.Mailbox()
                {
                    Email = mailbox.Name,
                    Password = mailbox.Password,
                    Quota = mailbox.Quota,
                    Id = id
                });
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error creating mailbox {mailbox.Name}.", ex);
            }
        }

        public async Task WriteMail(Mailbox mailbox, IMail mail)
        {
            using var context = new Models.MerelyMailContext();
            var mailBoxId = (await context.Mailboxes.Where(m => m.Email == mailbox.Name && m.Password == mailbox.Password).FirstOrDefaultAsync())?.Id ?? throw new Exception($"Password incorrect for source mailbox of name {mailbox.Name} or mailbox doesn't exist.");
            var folderId = (await context.Folders.Where(f => f.MailboxId == mailBoxId && f.Name == mail.Folder).FirstOrDefaultAsync())?.Id;
            if (folderId is null)
            {
                folderId = (await context.Folders.MaxAsync(f => f.Id)) + 1;
                await context.Folders.AddAsync(new Models.Folder()
                {
                    Id = folderId.Value,
                    MailboxId = mailBoxId,
                    Name = mail.Folder ?? "NewFolder" //This provider requires the folder name but the other not so much, so we gracefully create one for us.
                });
                await context.SaveChangesAsync();
            }
            var mailId = (await context.Mails.MaxAsync(m => m.Id)) + 1;
            await context.Mails.AddAsync(new Models.Mail()
            {
                Body = mail.Body,
                FolderId = folderId.Value,
                From = mail.Sender,
                Id = mailId,
                MailboxId = mailBoxId,
                Size = mail.Size,
                Subject = mail.Subject ?? "(no subject)",
                To = mail.Receiver
            });
            await context.SaveChangesAsync();
        }

        private static async Task<Models.Mailbox?> GetDbMailbox(Models.MerelyMailContext context, string username, string password)
        {
            return await context.Mailboxes.FirstOrDefaultAsync(m => m.Email == username && m.Password == password);
        }
    }
}
