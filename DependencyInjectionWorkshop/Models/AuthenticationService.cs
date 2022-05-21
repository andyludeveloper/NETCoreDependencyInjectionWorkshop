using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpProxy _otpProxy;
        private readonly SlackAdapter _slackAdapter;
        private readonly FailedCountProxy _failedCountProxy;
        private readonly NLogAdapter _nLogAdapter;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpProxy = new OtpProxy();
            _slackAdapter = new SlackAdapter();
            _failedCountProxy = new FailedCountProxy();
            _nLogAdapter = new NLogAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            //is account locked
            var isAccountLocked = _failedCountProxy.GetIsAccountLocked(accountId);
            if (isAccountLocked)
            {
                throw new FailedTooManyTimesException { AccountId = accountId };
            }

            var passwordFromDb = _profileDao.GetPasswordFromDb(accountId);
            var inputPassword = _sha256Adapter.GetHashedPassword(password);
            var otpFromApi = _otpProxy.GetCurrentOtp(accountId);

            if (passwordFromDb == inputPassword && otp == otpFromApi)
            {
                _failedCountProxy.ResetFailedCount(accountId);
                return true;
            }
            else
            {
                _failedCountProxy.AddFailedCount(accountId);

                var failedCount = _failedCountProxy.GetFailedCount(accountId);

                _nLogAdapter.LogFailedCount($"accountId:{accountId} failed times:{failedCount}");

                _slackAdapter.Notify("Login failure");
                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}