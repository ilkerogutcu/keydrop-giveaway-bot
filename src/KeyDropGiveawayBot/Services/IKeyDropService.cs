using KeyDropGiveawayBot.Models;

namespace KeyDropGiveawayBot.Services;

public interface IKeyDropService
{
    Task<List<Giveaway>> GetGiveawaysAsync();
}