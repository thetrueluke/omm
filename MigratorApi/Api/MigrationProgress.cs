namespace MigratorApi.Api
{
    public abstract class MigrationProgress
    {
        public abstract MigrationProgressType ProgressType { get; }
    }

    public enum MigrationProgressType
    {
        InProgress,
        NotFound
    }

    public class MigrationInProgress(uint remainingMessages, int totalMessages) : MigrationProgress
    {
        public override MigrationProgressType ProgressType => MigrationProgressType.InProgress;

        public uint RemainingMessages { get; } = remainingMessages;
        public int TotalMessages { get; } = totalMessages;
    }

    public class MigrationNotFound : MigrationProgress
    {
        public override MigrationProgressType ProgressType => MigrationProgressType.NotFound;
    }
}
