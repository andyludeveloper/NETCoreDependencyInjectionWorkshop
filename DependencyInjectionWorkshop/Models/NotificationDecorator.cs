namespace DependencyInjectionWorkshop.Models;

public class NotificationDecorator : IAuthentication
{
    private readonly IAuthentication _authenticationService;
    private readonly INotification _notification;

    public NotificationDecorator(IAuthentication authenticationService, INotification notification)
    {
        _authenticationService = authenticationService;
        _notification = notification;
    }

    public bool Verify(string accountId, string password, string otp)
    {
        var verify = _authenticationService.Verify(accountId, password, otp);
        if (!verify) NotifyWhenNotify();

        return verify;
    }

    private void NotifyWhenNotify()
    {
        _notification.Notify("Login failure");
    }
}