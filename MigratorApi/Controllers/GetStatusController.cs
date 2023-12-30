using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Migrator.Common;
using MigratorApi.Api;
using MigratorApi.Services;

namespace MigratorApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GetStatusController : ControllerBase
    {
        private readonly MessageInfoService messageInfoService;

        public GetStatusController(ILogger<BeginMigrationController> logger, MessageInfoService messageInfoService)
        {
            this.messageInfoService = messageInfoService;
        }

        [HttpPost]
        public async Task<IActionResult> GetStatus(MigrationSpec spec)
        {
            var sourceProvider = MailProviderFactory.GetMailProvier(spec.SourceMailProvider) ?? throw new ArgumentException($"No such mail provider: {spec.SourceMailProvider}");

            var mailbox = await sourceProvider.GetMailbox(spec.Mailbox.Name, spec.Mailbox.Password);

            var progress = messageInfoService.GetProgress(mailbox);
            if (progress.ProgressType == MigrationProgressType.NotFound)
            {
                return new NotFoundResult();
            }
            else
            {
                return new OkObjectResult(progress);
            }
        }
    }
}
