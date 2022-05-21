namespace DependencyInjectionWorkshop.Models;

public class FailedCountProxy
{
    public bool GetIsAccountLocked(string accountId, HttpClient httpClient)
    {
        var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).GetAwaiter()
            .GetResult();

        isLockedResponse.EnsureSuccessStatusCode();
        var isAccountLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
        return isAccountLocked;
    }

    public void AddFailedCount(string accountId, HttpClient httpClient)
    {
        var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;

        addFailedCountResponse.EnsureSuccessStatusCode();
    }

    public void ResetFailedCount(string accountId, HttpClient httpClient)
    {
        var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
        resetResponse.EnsureSuccessStatusCode();
    }


    public int GetFailedCount(string accountId, HttpClient httpClient)
    {
        var failedCountResponse =
            httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

        failedCountResponse.EnsureSuccessStatusCode();

        var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
        return failedCount;
    }
}