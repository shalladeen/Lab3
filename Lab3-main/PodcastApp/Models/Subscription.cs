namespace PodcastApp.Models;
public class Subscription
{
    public int SubscriptionID { get; set; }
    public string UserID { get; set; } = "";
    public int PodcastID { get; set; }
    public DateTime SubscribedDate { get; set; } = DateTime.UtcNow;
}
