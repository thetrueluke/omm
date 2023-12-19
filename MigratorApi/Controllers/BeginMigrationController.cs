using System.Security.Authentication;
using Microsoft.AspNetCore.Mvc;
using Migrator.Common;
using MigratorApi.Api;
using RabbitMQ.Client;

namespace MigratorApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BeginMigrationController : ControllerBase
    {
        public BeginMigrationController(ILogger<BeginMigrationController> logger)
        {
            //Required to load mail provider assemblies before they can be instantiated by the MailProviderFactory (this is not really a production approach).
            logger.LogInformation(new MerelyMailProvider.MerelyMailProvider().Name);
            logger.LogInformation(new AlmostMailProvider.AlmostMailProvider().Name);
        }

        //[HttpGet]
        //public IActionResult GetUsers()
        //{
        //    return Ok(users);
        //}

        [HttpPost]
        public async Task<IActionResult> BeginMigration(MigrationSpec spec)
        {
            try
            {
                var sourceProvider = MailProviderFactory.GetMailProvier(spec.SourceMailProvider) ?? throw new ArgumentException($"No such mail provider: {spec.SourceMailProvider}");
                var destinationProvider = MailProviderFactory.GetMailProvier(spec.DestinationMailProvider) ?? throw new ArgumentException($"No such mail provider: {spec.DestinationMailProvider}");

                var sourceMailbox = await sourceProvider.GetMailbox(spec.Mailbox.Name, spec.Mailbox.Password);
                await sourceProvider.CreateMailbox(spec.Mailbox);

                EnqueueMails(destinationProvider, sourceMailbox);

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
            catch (AuthenticationException authex)
            {
                return Unauthorized(authex.Message);
            }
        }

        private void EnqueueMails(IMailProvider destinationProvider, Mailbox sourceMailbox)
        {
            Task.Run(() =>
            {
                var factory = new ConnectionFactory { HostName =  Migration.HostName};
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(queue: Migration.QueueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var mails = destinationProvider.GetMails(sourceMailbox).Result;
                foreach (var mail in mails)
                {
                    var data = new MigrationData()
                    {
                        Mail = mail,
                        ProviderName = destinationProvider.Name
                    };
                    var body = data.Serialize();

                    channel.BasicPublish(exchange: string.Empty,
                                         routingKey: Migration.QueueName,
                                         basicProperties: null,
                                         body: body);
                }
            });
        }
    }
}
