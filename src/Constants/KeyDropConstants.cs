namespace KeyDropGiveawayBot.Constants;

public static class KeyDropConstants
{
    public const string KeyDropEndpoint = "https://key-drop.com";
    public const string GetGiveawayListEndpoint =
        "https://wss-2061.key-drop.com/v1/giveaway//list?type=active&page=0&perPage=5&status=active&sort=latest";

    public const string GetGiveawayResultEndpoint = "https://wss-2061.key-drop.com/v1/giveaway//data/{0}";
    public const string JoinGiveawayEndpoint = "https://wss-3002.key-drop.com/v1/giveaway//joinGiveaway/{0}";
    public const string GetTokenEndpoint = "https://key-drop.com/tr/token";
    public const string VioShieldEndpoint = "https://key-drop.com/cybervio/shield/pre-check";
}