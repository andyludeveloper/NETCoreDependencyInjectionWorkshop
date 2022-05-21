using DependencyInjectionWorkshop.Models;
using NSubstitute;

namespace DependencyInjectionWorkshopTests;

[TestFixture]
public class AuthenticationServiceTests
{
    private IFailedCounter _failedCounter;
    private IHash _hash;
    private ILog _log;
    private IOtp _otp;
    private IProfile _profile;
    private INotification _notification;

    [Test]
    public void is_valid()
    {
        _failedCounter = Substitute.For<IFailedCounter>();
        _hash = Substitute.For<IHash>();
        _log = Substitute.For<ILog>();
        _otp = Substitute.For<IOtp>();
        _profile = Substitute.For<IProfile>();
        _notification = Substitute.For<INotification>();
        var authenticationService =
            new AuthenticationService(_failedCounter, _hash, _log, _otp, _profile, _notification);
        _failedCounter.GetIsAccountLocked("andy").Returns(false);
        _profile.GetPasswordFromDb("andy").Returns("HASHPASSWORD");
        _hash.Compute("1234").Returns("HASHPASSWORD");
        _otp.GetCurrentOtp("andy").Returns("0000");

        var verify = authenticationService.Verify("andy", "1234", "0000");

        Assert.AreEqual(true, verify);
        _failedCounter.Received(1).Reset("andy");
    }
}