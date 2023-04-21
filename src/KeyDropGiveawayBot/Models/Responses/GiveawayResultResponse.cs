namespace KeyDropGiveawayBot.Models.Responses;

public class GiveawayResult
{
    public string Id { get; set; }
    public string? MySteamId { get; set; }
    public int? MaxUsers { get; set; }
    public int? MinUsers { get; set; }
    public int? DepositAmountRequired { get; set; }
    public int? DepositAmountMissing { get; set; }
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

public class Prize
{
    public int? Id { get; set; }
    public string Color { get; set; }
    public string ItemImg { get; set; }
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public double? Price { get; set; }
    public string Condition { get; set; }
    public string WeaponType { get; set; }
}

public class Userdata
{
    public string IdSteam { get; set; }
    public string Username { get; set; }
    public string SteamAvatar { get; set; }
    public int? Ticket { get; set; }
    public int? Slot { get; set; }
    public string ClientSeed { get; set; }
}

public class Winner
{
    public int? PrizeId { get; set; }
    public Userdata Userdata { get; set; }
}