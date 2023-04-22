namespace KeyDropGiveawayBot.Exceptions;

public class ExpiredCookieException : Exception
{
    public ExpiredCookieException(
        string message = "Your cookie has expired. Please update your cookie in the appsettings.json file.") :
        base(message)
    {
    }
}