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
