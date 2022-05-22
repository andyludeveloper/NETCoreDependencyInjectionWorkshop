namespace DependencyInjectionWorkshop.Models;

public interface IAuthentication
{
    bool Verify(string accountId, string password, string otp);
}

public class AuthenticationService : IAuthentication
{
    private readonly IHash _hash;
    private readonly IOtp _otp;
    private readonly IProfile _profile;

    public AuthenticationService(IProfile profile, IHash hash, IOtp otp)
    {
        _hash = hash;
        _otp = otp;
        _profile = profile;
    }

    public bool Verify(string accountId, string password, string otp)
    {
        var passwordFromDb = _profile.GetPasswordFromDb(accountId);
        var inputPassword = _hash.Compute(password);
        var otpFromApi = _otp.GetCurrentOtp(accountId);

        return passwordFromDb == inputPassword && otp == otpFromApi;
    }
}

public class FailedTooManyTimesException : Exception
{
    public string AccountId { get; set; }
}