namespace DependencyInjectionWorkshop.Models;

public interface ILog
{
    void LogFailedCount(string message);
}

public class NLogAdapter : ILog
{
    public NLogAdapter()
    {
    }

    public void LogFailedCount(string message)
    {
        var logger = NLog.LogManager.GetCurrentClassLogger();
        logger.Info(message);
    }
}