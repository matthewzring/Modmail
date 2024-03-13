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
