namespace KeyDropGiveawayBot.Services;

public interface ISessionService
{
    Task SetKeyDropCookieAsync(CancellationToken cancellationToken = default);
}