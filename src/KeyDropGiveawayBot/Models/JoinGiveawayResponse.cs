namespace KeyDropGiveawayBot.Models;

public class JoinGiveawayResponse
{
    public int? IdGiveaway { get; set; }
    public string IdSteam { get; set; }
    public string Username { get; set; }
    public string SteamAvatar { get; set; }
    public string ClientSeed { get; set; }
    public int? Ticket { get; set; }
    public int? Slot { get; set; }
}