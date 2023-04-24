namespace KeyDropGiveawayBot.Models;

public class GiveawayDetails
{
    public string Id { get; set; }
    public string? MySteamId { get; set; }
    public int? MaxUsers { get; set; }
    public int? MinUsers { get; set; }
    public int? DepositAmountRequired { get; set; }
    public double? DepositAmountMissing { get; set; }
    public string PublicHash { get; set; }
    public long? DeadlineTimestamp { get; set; }
    public string Status { get; set; }
    public List<Prize> Prizes { get; set; }
    public bool? CanIJoin { get; set; }
    public object BlockedUntil { get; set; }
    public bool? HaveIJoined { get; set; }
    public int? MySlot { get; set; }
    public List<string> Participants { get; set; }
    public int? ParticipantCount { get; set; }
    public string Frequency { get; set; }
    public List<Winner> Winners { get; set; }
}