namespace DependencyInjectionWorkshop.Models;

public class NotificationDecorator : IAuthentication
{
    private IAuthentication _authenticationService;
    private readonly INotification _notification;

    public NotificationDecorator(IAuthentication authenticationService, INotification notification)
    {
        _authenticationService = authenticationService;
        _notification = notification;
    }

    private void NotifyWhenNotify()
    {
        _notification.Notify("Login failure");
    }

    public bool Verify(string accountId, string password, string otp)
    {
        var verify = _authenticationService.Verify(accountId, password, otp);
        if (!verify)
        {
            NotifyWhenNotify();
        }

        return verify;
    }
}