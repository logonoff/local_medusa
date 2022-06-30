namespace local_medusa
{
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using Nadeko.Snake;

    /// <summary>
    /// Thanks to Kwoth#2452 on discord for this
    /// </summary>
    public class MyCommandContext : ICommandContext
    {
        public IDiscordClient Client { get; }
        public IGuild Guild { get; }
        public IMessageChannel Channel { get; }
        public IUser User { get; }
        public IUserMessage Message { get; }

        public MyCommandContext(GuildContext gctx, DiscordSocketClient client)
        {
            Client = client;
            Guild = gctx.Guild;
            Channel = gctx.Channel;
            User = gctx.User;
            Message = gctx.Message;
        }
    }
}
