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
            var mailbox = (await context.Mailboxes.FirstOrDefaultAsync(m => m.Email == username && m.Password == password)) ?? throw new KeyNotFoundException($"Mailbox with name {username} not found."); //ToDo: Add throwing on wrong password only.
            return new Mailbox()
            {
                Name = mailbox.Email,
                Password = password,
                Quota = mailbox.Quota,
            };
        }

        public Task<IEnumerable<IMail>> GetMails(Mailbox mailbox)
        {
            throw new NotImplementedException();
        }

        public Task WriteMail(Mailbox mailbox, IMail mail)
        {
            throw new NotImplementedException();
        }
    }
}
