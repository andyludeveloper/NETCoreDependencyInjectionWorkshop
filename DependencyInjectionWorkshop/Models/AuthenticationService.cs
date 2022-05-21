using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpProxy _otpProxy;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpProxy = new OtpProxy();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri("http://joey.com/") };

            //is account locked
            var isAccountLocked = GetIsAccountLocked(accountId, httpClient);
            if (isAccountLocked)
            {
                throw new FailedTooManyTimesException { AccountId = accountId };
            }

            var passwordFromDb = _profileDao.GetPasswordFromDb(accountId);
            var inputPassword = _sha256Adapter.GetHashedPassword(password);
            var otpFromApi = _otpProxy.GetCurrentOtp(accountId, httpClient);

            if (passwordFromDb == inputPassword && otp == otpFromApi)
            {
                ResetFailedCount(accountId, httpClient);
                return true;
            }
            else
            {
                AddFailedCount(accountId, httpClient);

                var failedCount = GetFailedCount(accountId, httpClient);

                LogFailedCount(accountId, failedCount);

                Notify("Login failure");
                return false;
            }
        }

        private static bool GetIsAccountLocked(string accountId, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).GetAwaiter()
                .GetResult();

            isLockedResponse.EnsureSuccessStatusCode();
            var isAccountLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isAccountLocked;
        }

        private static void Notify(string message)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(_ => { }, "my channel", message, "my bot name");
        }

        private static void LogFailedCount(string accountId, int failedCount)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }

        private static int GetFailedCount(string accountId, HttpClient httpClient)
        {
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        private static void AddFailedCount(string accountId, HttpClient httpClient)
        {
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;

            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        private static void ResetFailedCount(string accountId, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}