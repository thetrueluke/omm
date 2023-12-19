namespace Migrator.Common
{
    public interface IMailProvider
    {
        string Name { get; }


        Task<Mailbox> GetMailbox(string username, string password);

        Task<IEnumerable<IMail>> GetMails(Mailbox mailbox);


        Task CreateMailbox(Mailbox mailbox);

        Task WriteMail(Mailbox mailbox, IMail mail);
    }
}
