using KeyDropGiveawayBot.Models;

namespace KeyDropGiveawayBot.Services;

public interface IKeyDropService
{
    Task<List<Giveaway>?> GetGiveawaysAsync();
    Task<GiveawayDetail?> GetGiveawayDetailsByIdAsync(string giveawayId);
    Task<JoinGiveawayResponse?> JoinGiveawayAsync(string giveawayId);
}