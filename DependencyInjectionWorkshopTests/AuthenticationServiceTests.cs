using DependencyInjectionWorkshop.Models;
using NSubstitute;

namespace DependencyInjectionWorkshopTests;

[TestFixture]
public class AuthenticationServiceTests
{
    [SetUp]
    public void Setup()
    {
        _failedCounter = Substitute.For<IFailedCounter>();
        _hash = Substitute.For<IHash>();
        _log = Substitute.For<ILog>();
        _otp = Substitute.For<IOtp>();
        _profile = Substitute.For<IProfile>();
        _notification = Substitute.For<INotification>();
        _authenticationService = new AuthenticationService(_failedCounter, _hash, _log, _otp, _profile, _notification);
    }

    private IFailedCounter _failedCounter;
    private IHash _hash;
    private ILog _log;
    private IOtp _otp;
    private IProfile _profile;
    private INotification _notification;
    private AuthenticationService _authenticationService;

    [Test]
    public void valid()
    {
        WhenValid("andy");
        ShouldResetFailedCount("andy");
    }

    private void WhenValid(string accountId)
    {
        GivenIsAccountLocked(accountId, false);
        GivenPasswordFromDb(accountId, "HASHPASSWORD");
        GivenHashedPassword("1234", "HASHPASSWORD");
        GivenCurrentOtp(accountId, "0000");
        var verify = _authenticationService.Verify(accountId, "1234", "0000");
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