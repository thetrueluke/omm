using System.Text;
using Migrator.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var quitEvent = new ManualResetEvent(false);
Console.CancelKeyPress += (sender, e) =>
{
    quitEvent.Set();
    e.Cancel = true;
};
//Required to load mail provider assemblies before they can be instantiated by the MailProviderFactory (this is not really a production approach).
Console.WriteLine(new MerelyMailProvider.MerelyMailProvider().Name);
Console.WriteLine(new AlmostMailProvider.AlmostMailProvider().Name);

var factory = new ConnectionFactory { HostName = Migration.HostName };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: Migration.QueueName,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    try
    {
        Console.WriteLine($"Received message of length {ea.Body.Length}");
        var body = ea.Body.ToArray();
        var data = MigrationData.Deserialize(body);
        if (data is not null)
        {
            var destinationProvider = MailProviderFactory.GetMailProvier(data.ProviderName) ?? throw new Exception($"No such mail provider: {data.ProviderName}");
            destinationProvider.WriteMail(data.Mailbox, data.Mail);
            Console.WriteLine($"Added mail to mailbox {data.Mailbox.Name}");
        }
        else
        {
            throw new Exception("Message is not of a valid format (deserialization failed).");
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex.Message);
    }
};
channel.BasicConsume(queue: Migration.QueueName,
                     autoAck: true,
                     consumer: consumer);

quitEvent.WaitOne();
