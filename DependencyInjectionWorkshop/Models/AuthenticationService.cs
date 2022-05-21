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

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpProxy = new OtpProxy();
            _slackAdapter = new SlackAdapter();
            _failedCountProxy = new FailedCountProxy();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri("http://joey.com/") };

            //is account locked
            var isAccountLocked = _failedCountProxy.GetIsAccountLocked(accountId, httpClient);
            if (isAccountLocked)
            {
                throw new FailedTooManyTimesException { AccountId = accountId };
            }

            var passwordFromDb = _profileDao.GetPasswordFromDb(accountId);
            var inputPassword = _sha256Adapter.GetHashedPassword(password);
            var otpFromApi = _otpProxy.GetCurrentOtp(accountId, httpClient);

            if (passwordFromDb == inputPassword && otp == otpFromApi)
            {
                _failedCountProxy.ResetFailedCount(accountId, httpClient);
                return true;
            }
            else
            {
                _failedCountProxy.AddFailedCount(accountId, httpClient);

                var failedCount = _failedCountProxy.GetFailedCount(accountId, httpClient);

                LogFailedCount(accountId, failedCount);

                _slackAdapter.Notify("Login failure");
                return false;
            }
        }

        private void LogFailedCount(string accountId, int failedCount)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}