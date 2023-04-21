namespace KeyDropGiveawayBot.Models;

public class UserData
{
    public string IdSteam { get; set; }
    public string Username { get; set; }
    public string SteamAvatar { get; set; }
    public int? Ticket { get; set; }
    public int? Slot { get; set; }
    public string ClientSeed { get; set; }
}