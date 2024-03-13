namespace Modmail.utils;

/**
 * Modelled after jagrosh's LogUtil in Vortex
 */
public class LogUtil
{
    private const string LOG_TIME = "`[{0}]`";
    private const string EMOJI = " {1}";

    private const string LOG_FORMAT = LOG_TIME + EMOJI + " {2}";

    private const string GUILD_NAME = "{0}";
    private const string GUILD_ID = " | {1}";
    private const string MESSAGE_TIME = " • {2}";

    private const string FOOTER_FORMAT = GUILD_NAME + GUILD_ID + MESSAGE_TIME;

    public static string LogFormat(DateTimeOffset time, string emoji, string content)
    {
        return String.Format(LOG_FORMAT, LogTimeF(time), emoji, content);
    }

    public static string FooterFormat(string guild, ulong id, DateTimeOffset time)
    {
        return String.Format(FOOTER_FORMAT, guild, id, FooterTimeF(time));
    }

    private static string LogTimeF(DateTimeOffset time)
    {
        return time.ToLocalTime().ToString("HH:mm:ss");
    }

    private static string FooterTimeF(DateTimeOffset time)
    {
        return time.ToLocalTime().ToString("MM/dd/yyyy hh:mm tt");
    }
}
