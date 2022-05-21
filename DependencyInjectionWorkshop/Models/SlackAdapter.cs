using SlackAPI;

namespace DependencyInjectionWorkshop.Models;

public interface INotification
{
    void Notify(string message);
}

public class SlackAdapter : INotification
{
    public SlackAdapter()
    {
    }

    public void Notify(string message)
    {
        var slackClient = new SlackClient("my api token");
        slackClient.PostMessage(_ => { }, "my channel", message, "my bot name");
    }
}