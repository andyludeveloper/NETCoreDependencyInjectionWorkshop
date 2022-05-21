using System.Data;
using System.Data.SqlClient;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string accountId, string password, string otp)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri("http://joey.com/") };

            //is account locked
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).GetAwaiter().GetResult();

            isLockedResponse.EnsureSuccessStatusCode();
            if (isLockedResponse.Content.ReadAsAsync<bool>().Result)
            {
                throw new FailedTooManyTimesException { AccountId = accountId };
            }

            //password DB
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            //hash password
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var inputPassword = hash.ToString();

            //getOTP
            var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            string otpFromApi;
            if (response.IsSuccessStatusCode)
            {
                otpFromApi = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            if (passwordFromDb == inputPassword && otp == otpFromApi)
            {
                var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
                resetResponse.EnsureSuccessStatusCode();
                return true;
            }
            else
            {
                //Add failed count
                var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;

                addFailedCountResponse.EnsureSuccessStatusCode();
                
                //Get failed count
                var failedCountResponse =
                    httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

                failedCountResponse.EnsureSuccessStatusCode();

                var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
                
                // Log failed count
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Info($"accountId:{accountId} failed times:{failedCount}");

                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(_ => { }, "my channel", "Login failure", "my bot name");
                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}