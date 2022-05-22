namespace DependencyInjectionWorkshop.Models;

public class LogDecorator : IAuthentication
{
    private readonly IAuthentication _authenticationService;
    private readonly IFailedCounter _failedCounter;
    private readonly ILog _log;

    public LogDecorator(IAuthentication authenticationService, IFailedCounter failedCounter, ILog log)
    {
        _authenticationService = authenticationService;
        _failedCounter = failedCounter;
        _log = log;
    }

    public bool Verify(string accountId, string password, string otp)
    {
        var verify = _authenticationService.Verify(accountId, password, otp);
        if (!verify) LogFailedCount(accountId);

        return verify;
    }

    private void LogFailedCount(string accountId)
    {
        var failedCount = _failedCounter.Get(accountId);

        _log.LogFailedCount($"accountId:{accountId} failed times:{failedCount}");
    }
}