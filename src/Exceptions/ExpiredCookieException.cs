namespace KeyDropGiveawayBot.Exceptions;

public class ExpiredCookieException : Exception
{
    public ExpiredCookieException(
        string message = "Your cookie has expired.") :
        base(message)
    {
    }
}