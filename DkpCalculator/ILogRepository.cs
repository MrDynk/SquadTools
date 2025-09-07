namespace Logs
{
    public interface ILogRepository
    {
        // Retrieves preliminary data points from the logs and updates the context
        void GetPriliminaryDataPoints(SquadSheetContext context);

        // Retrieves player activity from the logs and updates the context
        void GetPlayerActivity(SquadSheetContext context);

        void FindFirstActivityPriorToEventTime(string identifier, DateTime eventTime);
    }
}
