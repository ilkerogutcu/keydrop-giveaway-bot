namespace KeyDropGiveawayBot.Models;

public class Giveaway
{
    public string Id { get; set; }
    public string Status { get; set; }
    public int? MaxUsers { get; set; }
    public int? MinUsers { get; set; }
    public bool? HaveIJoined { get; set; }
    public int? MySlot { get; set; }
    public string PublicHash { get; set; }
    public object DeadlineTimestamp { get; set; }
    public string Frequency { get; set; }
    public List<Prize> Prizes { get; set; }
    public int? ParticipantCount { get; set; }
    public List<Winner> Winners { get; set; }
}