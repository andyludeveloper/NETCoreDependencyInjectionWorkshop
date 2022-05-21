using SlackAPI;

namespace DependencyInjectionWorkshop.Models;

public class SlackAdapter
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