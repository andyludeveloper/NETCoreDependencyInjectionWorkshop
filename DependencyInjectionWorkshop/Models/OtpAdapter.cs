namespace DependencyInjectionWorkshop.Models;

public class OtpAdapter
{
    public string GetCurrentOtp(string accountId, HttpClient httpClient)
    {
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

        return otpFromApi;
    }
}