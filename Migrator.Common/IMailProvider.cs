namespace Migrator.Common
{
    public interface IMailProvider
    {
        Mailbox GetMailbox(string username, string password);

        IEnumerable<IMail> GetMails(Mailbox mailbox);


        Mailbox CreateMailbox(string username, string password);

        void WriteMail(Mailbox mailbox, IMail mail);
    }
}
