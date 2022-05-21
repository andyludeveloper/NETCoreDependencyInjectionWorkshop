namespace DependencyInjectionWorkshop.Models;

public interface IOtp
{
    string GetCurrentOtp(string accountId);
}

public class Otp : IOtp
{
    public string GetCurrentOtp(string accountId)
    {
        var response = new HttpClient { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/otps", accountId).Result;
        string otpFromApi;
        if (response.IsSuccessStatusCode)
        {
            otpFromApi = response.Content.ReadAsAsync<string>().Result;
        }
        else
        {
            throw new Exception($"web api error, accountId:{accountId}");
        }

        return otpFromApi;
    }
}