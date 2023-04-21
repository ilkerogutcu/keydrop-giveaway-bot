namespace KeyDropGiveawayBot.Constants;

public static class KeyDropEndpoints
{
    public const string GetGiveawayListEndpoint = "https://wss-2061.key-drop.com/v1/giveaway//list?type=active&page=0&perPage=5&status=active&sort=latest";
    public const string GetGiveawayResultEndpoint = "https://wss-2061.key-drop.com/v1/giveaway//data/{0}";
}