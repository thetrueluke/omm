using System.Security.Authentication;
using Microsoft.AspNetCore.Mvc;
using Migrator.Common;
using MigratorApi.Api;
using MigratorApi.Services;
using RabbitMQ.Client;

namespace MigratorApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BeginMigrationController : ControllerBase
    {
        private readonly MessageInfoService messageInfoService;

        public BeginMigrationController(ILogger<BeginMigrationController> logger, MessageInfoService messageInfoService)
        {
            //Required to load mail provider assemblies before they can be instantiated by the MailProviderFactory (this is not really a production approach).
            logger.LogInformation(new MerelyMailProvider.MerelyMailProvider().Name);
            logger.LogInformation(new AlmostMailProvider.AlmostMailProvider().Name);
            this.messageInfoService = messageInfoService;
        }

        [HttpPost]
        public async Task<IActionResult> BeginMigration(MigrationSpec spec)
        {
            try
            {
                var sourceProvider = MailProviderFactory.GetMailProvier(spec.SourceMailProvider) ?? throw new ArgumentException($"No such mail provider: {spec.SourceMailProvider}");
                var destinationProvider = MailProviderFactory.GetMailProvier(spec.DestinationMailProvider) ?? throw new ArgumentException($"No such mail provider: {spec.DestinationMailProvider}");

                var mailbox = await sourceProvider.GetMailbox(spec.Mailbox.Name, spec.Mailbox.Password);
                if (messageInfoService.IsMailboxAlreadyAdded(mailbox))
                {
                    throw new ArgumentException($"Mailbox {spec.Mailbox.Name} is already being processed.");
                }
                await destinationProvider.CreateMailbox(mailbox);

                EnqueueMails(sourceProvider, destinationProvider, mailbox);

                return Ok();
            }
            catch (InvalidOperationException iopex) 
            {
                return BadRequest(iopex.Message);
            }
            catch (ArgumentException argex)
            {
                return BadRequest(argex.Message);
            }
            catch (KeyNotFoundException knfex)
            {
                return NotFound(knfex.Message);
            }
            catch (UnauthorizedAccessException authex)
            {
                return Unauthorized(authex.Message);
            }
        }

        private void EnqueueMails(IMailProvider sourceProvider, IMailProvider destinationProvider, Mailbox mailbox)
        {
            Task.Run(() =>
            {
                var factory = new ConnectionFactory { HostName =  Migration.HostName};
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();
                
                channel.ConfirmSelect();

                channel.QueueDeclare(queue: Migration.QueueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var mails = sourceProvider.GetMails(mailbox).Result.ToList();

                messageInfoService.AddChannel(mailbox, (channel, mails.Count));

                foreach (var mail in mails)
                {
                    var data = new MigrationData()
                    {
                        Mail = mail,
                        Mailbox = mailbox,
                        ProviderName = destinationProvider.Name
                    };
                    var body = data.Serialize();

                    channel.BasicPublish(exchange: string.Empty,
                                         routingKey: Migration.QueueName,
                                         basicProperties: null,
                                         body: body);
                }

                channel.WaitForConfirms();
                messageInfoService.RemoveChannel(mailbox);
            });
        }
    }
}
