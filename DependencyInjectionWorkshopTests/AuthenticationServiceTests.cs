using DependencyInjectionWorkshop.Models;
using NSubstitute;

namespace DependencyInjectionWorkshopTests;

[TestFixture]
public class AuthenticationServiceTests
{
    [SetUp]
    public void SetUp()
    {
        _failedCounter = Substitute.For<IFailedCounter>();
        _hash = Substitute.For<IHash>();
        _log = Substitute.For<ILog>();
        _otp = Substitute.For<IOtp>();
        _profile = Substitute.For<IProfile>();
        _notification = Substitute.For<INotification>();
        _authenticationService = new AuthenticationService(_failedCounter, _hash, _log, _otp, _profile);
        _authenticationService = new NotificationDecorator(_authenticationService, _notification);
    }

    private IFailedCounter _failedCounter;
    private IHash _hash;
    private ILog _log;
    private IOtp _otp;
    private IProfile _profile;
    private INotification _notification;
    private IAuthentication _authenticationService;

    [Test]
    public void valid()
    {
        GivenIsAccountLocked("andy", false);
        GivenPasswordFromDb("andy", "HASHPASSWORD");
        GivenHashedPassword("1234", "HASHPASSWORD");
        GivenCurrentOtp("andy", "0000");

        ShouldBeValid("andy", "1234", "0000");
        // ShouldResetFailedCount("andy");
    }

    [Test]
    public void password_incorrect()
    {
        GivenIsAccountLocked("andy", false);
        GivenPasswordFromDb("andy", "HASHPASSWORD");
        GivenHashedPassword("1234", "HASHPASSWORD");
        GivenCurrentOtp("andy", "0000");

        ShouldBeInvalid("andy");
    }

    [Test]
    public void invalid_should_failed_count_add()
    {
        GivenInvalid("andy");

        _failedCounter.Received().Add("andy");
    }

    [Test]
    public void invalid_should_notify()
    {
        GivenInvalid("andy");
        _notification.Received().Notify("Login failure");
    }

    [Test]
    public void invalid_should_log()
    {
        GivenFailedCount("andy", 5);
        GivenInvalid("andy");
        ShouldLog("andy", 5);
        // _notification.Received().Notify("Login failure");
    }

    private void ShouldLog(string accountId, int counter)
    {
        _log.Received().LogFailedCount(Arg.Is<string>(s => s.Contains(accountId) && s.Contains(counter.ToString())));
    }

    private void GivenFailedCount(string accountId, int returnThis)
    {
        _failedCounter.Get(accountId).Returns(returnThis);
    }

    private void GivenInvalid(string accountId)
    {
        GivenIsAccountLocked(accountId, false);
        GivenPasswordFromDb(accountId, "HASHPASSWORD");
        GivenHashedPassword("1234", "wrong password");
        GivenCurrentOtp(accountId, "0000");
        ShouldBeInvalid(accountId);
    }

    private void ShouldBeInvalid(string accountId)
    {
        var verify = _authenticationService.Verify(accountId, "wrong password", "0000");
        Assert.AreEqual(false, verify);
    }


    [Test]
    public void reset_failed_count_when_valid()
    {
        WhenValid("andy");
        ShouldResetFailedCount("andy");
    }

    [Test]
    public void account_is_locked()
    {
        GivenIsAccountLocked("andy", true);

        ShouldThrowFailedTooManyTimesException<FailedTooManyTimesException>("andy");
    }

    private void ShouldThrowFailedTooManyTimesException<TException>(string accountId) where TException : Exception
    {
        Assert.Throws<TException>(() =>
            _authenticationService.Verify(accountId, "1234", "0000")
        );
    }

    private void WhenValid(string accountId)
    {
        GivenIsAccountLocked(accountId, false);
        GivenPasswordFromDb(accountId, "HASHPASSWORD");
        GivenHashedPassword("1234", "HASHPASSWORD");
        GivenCurrentOtp(accountId, "0000");

        _authenticationService.Verify(accountId, "1234", "0000");
    }

    private void ShouldBeValid(string accountId, string password, string otp)
    {
        var verify = _authenticationService.Verify(accountId, password, otp);
        Assert.AreEqual(true, verify);
    }

    private void ShouldResetFailedCount(string accountId)
    {
        _failedCounter.Received(1).Reset(accountId);
    }

    private void GivenCurrentOtp(string accountId, string returnThis)
    {
        _otp.GetCurrentOtp(accountId).Returns(returnThis);
    }

    private void GivenHashedPassword(string password, string returnThis)
    {
        _hash.Compute(password).Returns(returnThis);
    }

    private void GivenPasswordFromDb(string accountId, string returnThis)
    {
        _profile.GetPasswordFromDb(accountId).Returns(returnThis);
    }

    private void GivenIsAccountLocked(string accountId, bool returnThis)
    {
        _failedCounter.GetIsAccountLocked(accountId).Returns(returnThis);
    }
}