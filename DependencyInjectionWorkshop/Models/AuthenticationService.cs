namespace DependencyInjectionWorkshop.Models;

public interface IAuthentication
{
    bool Verify(string accountId, string password, string otp);
}

public class AuthenticationService : IAuthentication
{
    // private readonly IFailedCounter _failedCounter;
    private readonly IHash _hash;

    // private readonly ILog _log;

    // private readonly INotification _notification;
    private readonly IOtp _otp;

    private readonly IProfile _profile;
    private readonly LogDecorator _logDecorator;

    // private readonly FailedCounterDecorator _failedCounterDecorator;
    // private readonly NotificationDecorator _notificationDecorator;

    public AuthenticationService(IHash hash, ILog log, IOtp otp, IProfile profile
    )
    {
        _hash = hash;
        // _log = log;
        _otp = otp;
        _profile = profile;
        // _logDecorator = new LogDecorator(this);
    }

    // public AuthenticationService()
    // {
    //     _profile = new ProfileDao();
    //     _hash = new Sha256Adapter();
    //     _otp = new Otp();
    //     // _notification = new SlackAdapter();
    //     _failedCounter = new FailedCounter();
    //     _log = new NLogAdapter();
    //     new NotificationDecorator(this, new SlackAdapter());
    // }

    public bool Verify(string accountId, string password, string otp)
    {
        var passwordFromDb = _profile.GetPasswordFromDb(accountId);
        var inputPassword = _hash.Compute(password);
        var otpFromApi = _otp.GetCurrentOtp(accountId);

        return passwordFromDb == inputPassword && otp == otpFromApi;

        // _logDecorator.LogFailedCount(accountId);

        // _notificationDecorator.NotifyWhenNotify();
    }
}

public class FailedTooManyTimesException : Exception
{
    public string AccountId { get; set; }
}