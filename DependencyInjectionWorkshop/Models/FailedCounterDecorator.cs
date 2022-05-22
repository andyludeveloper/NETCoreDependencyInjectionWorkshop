namespace DependencyInjectionWorkshop.Models;

public class FailedCounterDecorator : IAuthentication
{
    private readonly IAuthentication _authenticationService;
    private readonly IFailedCounter _failedCounter;

    public FailedCounterDecorator(IAuthentication authenticationService, IFailedCounter failedCounter)
    {
        _authenticationService = authenticationService;
        _failedCounter = failedCounter;
    }

    public bool Verify(string accountId, string password, string otp)
    {
        CheckAccountLocked(accountId);

        var isValid = _authenticationService.Verify(accountId, password, otp);
        if (isValid)
        {
            Reset(accountId);
        }
        else
        {
            Add(accountId);
        }
        return isValid;
    }

    private void CheckAccountLocked(string accountId)
    {
        var isAccountLocked = _failedCounter.GetIsAccountLocked(accountId);
        if (isAccountLocked) throw new FailedTooManyTimesException { AccountId = accountId };
    }
    private void Reset(string accountId)
    {
        _failedCounter.Reset(accountId);
    }
    
    private void Add(string accountId)
    {
        _failedCounter.Add(accountId);
    }
}