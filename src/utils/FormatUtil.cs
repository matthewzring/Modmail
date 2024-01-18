using Discord;
using System.Text;

namespace Modmail.utils
{
    public class FormatUtil
    {
        public static string filterEveryone(string input)
        {
            return input.Replace("\u202E", "") // RTL override
                    .Replace("@everyone", "@\u0435veryone") // cyrillic e
                    .Replace("@here", "@h\u0435re") // cyrillic e
                    .Replace("discord.gg/", "discord\u2024gg/") // one dot leader
                    .Replace("@&", "\u0DB8&"); // role failsafe
        }

        public static string formatUser(IUser user)
        {
            if (user.Discriminator == "0000")
                return filterEveryone($"**{user.Username}**");
            return filterEveryone($"**{user.Username}**#{user.Discriminator}");
        }

        public static string formatFullUser(IUser user)
        {
            if (user.Discriminator == "0000")
                return filterEveryone($"**{user.Username}** (ID:{user.Id})");
            return filterEveryone($"**{user.Username}**#{user.Discriminator} (ID:{user.Id})");
        }
    }
}
