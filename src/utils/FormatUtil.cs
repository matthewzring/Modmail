/*
 * Copyright 2023-2024 Matthew Ring
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Discord;

namespace Modmail.utils;

/**
 * Modelled after jagrosh's FormatUtil in Vortex
 */
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
