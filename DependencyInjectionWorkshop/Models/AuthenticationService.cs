﻿namespace DependencyInjectionWorkshop.Models;

public class AuthenticationService
{
    private readonly IFailedCounter _failedCounter;
    private readonly IHash _hash;
    private readonly ILog _log;
    private readonly INotification _notification;
    private readonly IOtp _otp;
    private readonly IProfile _profile;

    public AuthenticationService(IFailedCounter failedCounter, IHash hash, ILog log, IOtp otp, IProfile profile,
        INotification notification)
    {
        _failedCounter = failedCounter;
        _hash = hash;
        _log = log;
        _otp = otp;
        _profile = profile;
        _notification = notification;
    }

    public AuthenticationService()
    {
        _profile = new ProfileDao();
        _hash = new Sha256Adapter();
        _otp = new Otp();
        _notification = new SlackAdapter();
        _failedCounter = new FailedCounter();
        _log = new NLogAdapter();
    }

    public bool Verify(string accountId, string password, string otp)
    {
        //is account locked
        var isAccountLocked = _failedCounter.GetIsAccountLocked(accountId);
        if (isAccountLocked) throw new FailedTooManyTimesException { AccountId = accountId };

        var passwordFromDb = _profile.GetPasswordFromDb(accountId);
        var inputPassword = _hash.Compute(password);
        var otpFromApi = _otp.GetCurrentOtp(accountId);

        if (passwordFromDb == inputPassword && otp == otpFromApi)
        {
            _failedCounter.Reset(accountId);
            return true;
        }

        _failedCounter.Add(accountId);

        var failedCount = _failedCounter.Get(accountId);

        _log.LogFailedCount($"accountId:{accountId} failed times:{failedCount}");

        _notification.Notify("Login failure");
        return false;
    }
}

public class FailedTooManyTimesException : Exception
{
    public string AccountId { get; set; }
}