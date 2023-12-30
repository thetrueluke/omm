using System.Collections.Concurrent;
using Migrator.Common;
using MigratorApi.Api;
using RabbitMQ.Client;

namespace MigratorApi.Services
{
    public class MessageInfoService
    {
        private readonly ConcurrentDictionary<Mailbox, (IModel model, int totalMessageCount)> channels = [];

        public void AddChannel(Mailbox mailbox, (IModel, int) modelAndTotalMessageCount)
        {
            channels.TryAdd(mailbox, modelAndTotalMessageCount);
        }

        public void RemoveChannel(Mailbox mailbox)
        {
            channels.Remove(mailbox, out _);
        }

        public bool IsMailboxAlreadyAdded(Mailbox mailbox)
        {
            return channels.ContainsKey(mailbox);
        }

        public MigrationProgress GetProgress(Mailbox mailbox)
        {
            if (channels.TryGetValue(mailbox, out var modelAndTotalMessageCount))
            {
                return new MigrationInProgress(modelAndTotalMessageCount.model.MessageCount(Migration.QueueName), modelAndTotalMessageCount.totalMessageCount);
            }
            else
            {
                return new MigrationNotFound();
            }
        }
    }
}
