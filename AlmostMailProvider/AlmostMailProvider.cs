using Migrator.Common;

namespace AlmostMailProvider
{
    public class AlmostMailProvider : IMailProvider
    {
        public string Name => GetType().Name.Replace("Provider", "");

        public Task CreateMailbox(Mailbox mailbox)
        {
            throw new NotImplementedException();
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
    }
}
