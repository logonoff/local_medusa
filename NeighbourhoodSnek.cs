namespace local_medusa
{
    using Nadeko.Snake;
    using Discord.Commands;
    using Discord.WebSocket;
    using NadekoBot.Modules.Music.Services;
    using NadekoBot;
    using Discord;
    using static PathInteractionHelper;

    public sealed class NeighbourhoodSnek : Snek
    {
        private readonly CommandService _cmds;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private readonly IMusicService _musicService;

        public NeighbourhoodSnek(
            CommandService cmds,
            DiscordSocketClient client,
            IServiceProvider services,
            IMusicService musicService)
        {
            _cmds = cmds;
            _client = client;
            _services = services;
            _musicService = musicService;
        }

        /// <summary>
        /// Queues local playlist or file while replacing mounted network path with direct UNC path.
        /// If it is a folder, it will recursively queue all files within that folder to a recursion depth of 5
        /// </summary>
        [cmd(aliases: new string[] { "playnetwork", "networkplay", "npl" })]
        [RequireOwner]
        public async Task NetworkPlay(GuildContext ctx, [leftover] string msg)
        {
            string path = UNCPath(msg);

            // determine if the path is a directory or a file        
            if (Directory.Exists(path))
            {
                string[] dirs = GetAllFolders(path);
                foreach (string subdir in dirs)
                {
                    await Execute(ctx, "localplaylist", UNCPath(subdir));
                }
            }
            else if (File.Exists(path))
            {
                await Execute(ctx, "local", UNCPath(path));
            }
            else
            {
                // await ctx.SendErrorAsync("**" + ctx.User.ToString() + $"** Track ``" + path + "`` not found");
                await ctx.ErrorLocalizedAsync("track_not_found", path);
                return;
            }
        }

        /// <summary>
        /// Queues WPL file with converted direct UNC path,
        /// does not check for bot owner permission!
        /// </summary>
        [cmd(aliases: new string[] { "playwpl", "wplplay", "wpl" })]
        [RequireOwner]
        public async Task PlayWPL(GuildContext ctx, [leftover] string path)
        {
            WPLParser playlist = new(UNCPath(path));
            List<string> songs = playlist.GetSongs(); // list of file paths

            // check the "succ"???
            var succ = await QueuePreconditionInternalAsync(ctx);
            if (!succ) { return; }

            // queue each track seperately
            foreach (var song in songs)
            {
                await PlayLocalTrack(ctx, song);
            }

            // await ctx.SendConfirmAsync("**" + ctx.User.ToString() + $"** Queued " + Path.GetFileName(path));
            await ctx.ReplyConfirmLocalizedAsync("dir_queue_complete");
        }

        /// <summary>
        /// Silently enqueue a file to the music queue
        /// Precondition: QueuePreconditionInternalAsync is assumed to be true
        /// </summary>
        private async Task PlayLocalTrack(GuildContext ctx, [leftover] string path)
        {
            // instantiate music player and fail silently if it didnt work
            var mp = await _musicService.GetOrCreateMusicPlayerAsync(ctx.Channel);

            if (mp is null)
            {
                // await ctx.ReplyErrorLocalizedAsync("no_player");
                return;
            }

            // enqueue and check if track was found
            var (trackInfo, _) = await mp.TryEnqueueTrackAsync(path, ctx.User.ToString(), false, NadekoBot.Modules.Music.MusicPlatform.Local);
            if (trackInfo is null)
            {
                // await ctx.SendErrorAsync("**" + ctx.User.ToString() + $"** Track ``" + path + "`` not found");
                await ctx.ErrorLocalizedAsync("track_not_found", path);
                return;
            }
        }

        /// <summary>
        /// Silently execute a front end bot command
        /// Thanks to Kwoth#2452 on discord for this
        /// </summary>
        private async Task Execute(GuildContext gctx, string CommandName, [leftover] string args = "")
        {
            var cmd = _cmds.Commands
                                   .FirstOrDefault(x => x.Name.Equals(CommandName, StringComparison.InvariantCultureIgnoreCase));

            var val = new TypeReaderValue(args, 100);
            await cmd.ExecuteAsync(new MyCommandContext(gctx, _client),
                ParseResult.FromSuccess(new[] { val }, Array.Empty<TypeReaderValue>()),
                _services);
        }

        /// <summary>
        /// Port of Music.cs private async
        /// </summary>
        /// <returns>If queueing is possible</returns>
        private async Task<bool> QueuePreconditionInternalAsync(GuildContext ctx)
        {
            var user = (IGuildUser)ctx.User;
            var voiceChannelId = user.VoiceChannel?.Id;

            if (voiceChannelId is null)
            {
                await ctx.ReplyErrorLocalizedAsync("must_be_in_voice");
                return false;
            }

            _ = ctx.Channel.TriggerTypingAsync();

            var botUser = await ctx.Guild.GetCurrentUserAsync();

            if (botUser.VoiceChannel?.Id is null && voiceChannelId is not null)
            {
                await _musicService.JoinVoiceChannelAsync(ctx.Guild.Id, (ulong)voiceChannelId);
            }
            else if (botUser.VoiceChannel?.Id != voiceChannelId)
            {
                await ctx.ReplyErrorLocalizedAsync("not_with_bot_in_voice");
                return false;
            }

            return true;
        }

    }
}
