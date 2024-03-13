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
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Modmail.services;

public class CommandHandlingService
{
    private readonly DiscordSocketClient _discord;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;

    public CommandHandlingService(IServiceProvider services)
    {
        _commands = services.GetRequiredService<CommandService>();
        _discord = services.GetRequiredService<DiscordSocketClient>();
        _services = services;

        // Hook CommandExecuted to handle post-command-execution logic.
        _commands.CommandExecuted += CommandExecutedAsync;
        // Hook MessageReceived so we can process each message to see if it qualifies as a command.
        _discord.MessageReceived += MessageReceivedAsync;
    }

    public async Task InitializeAsync()
    {
        // Register modules that are public and inherit ModuleBase<T>.
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    public async Task MessageReceivedAsync(SocketMessage rawMessage)
    {
        // Ignore system messages, or messages from other bots
        if (rawMessage is not SocketUserMessage message)
            return;
        if (message.Source != MessageSource.User)
            return;

        // This value holds the offset where the prefix ends
        int argPos = 0;

        // Perform prefix check
        if (!message.HasCharPrefix('!', ref argPos))
            return;

        // Create a WebSocket-based command context based on the message
        SocketCommandContext context = new SocketCommandContext(_discord, message);

        // Execute the command with the command context we just
        // created, along with the service provider for precondition checks.
        await _commands.ExecuteAsync(context, argPos, _services);
        // Note that normally a result will be returned by this format, but here
        // we will handle the result in CommandExecutedAsync,
    }

    public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        // Command is unspecified when there was a search failure (command not found); we don't care about these errors
        if (!command.IsSpecified)
            return;

        // The command was successful, we don't care about this result, unless we want to log that a command succeeded.
        if (result.IsSuccess)
            return;

        // The command failed, let's notify the user that something happened.
        await context.Channel.SendMessageAsync("An error occured: Please contact **matthewzring** for help.");
    }
}
