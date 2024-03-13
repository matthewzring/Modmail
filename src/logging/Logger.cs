using Discord;
using Modmail.utils;

namespace Modmail.logging;

/**
 * Modelled after jagrosh's BasicLogger in Vortex
 */
public class Logger
{
    public static async Task Log(DateTimeOffset now, ITextChannel tc, string emote, string message, Embed embed)
    {
        try
        {
            await tc.SendMessageAsync(FormatUtil.filterEveryone(LogUtil.LogFormat(now, emote, message)), embed: embed);
        }
        catch (Exception) { }
    }
}
