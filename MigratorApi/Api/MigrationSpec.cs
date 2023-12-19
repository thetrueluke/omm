using Migrator.Common;

namespace MigratorApi.Api
{
    public record class MigrationSpec
    {
        public required string SourceMailProvider { get; init; }

        public required string DestinationMailProvider { get; init; }

        public required Mailbox Mailbox { get; init; }
    }
}
