using System.Text;

namespace KeyDropGiveawayBot.Extensions;

public static class StringBuilderExtension
{
    public static string RemoveLast(this StringBuilder builder)
    {
        return builder.Length == 0
            ? string.Empty
            : builder.Remove(builder.Length - 1, 1).ToString();
    }
}