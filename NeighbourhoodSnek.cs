using Nadeko.Snake;
using NadekoBot.Services;

public sealed class NeighbourhoodSnek : Snek
{
    private readonly CommandHandler _cmdHandler;

    public NeighbourhoodSnek (
        CommandHandler cmdHandler
    )
    {
        _cmdHandler = cmdHandler;
    }

    /// <summary>
    /// Queues local playlist while replacing mounted network path with direct UNC path
    /// </summary>
    [cmd]
    public async Task NetworkPlay(GuildContext ctx, [leftover] string msg)
    {
        string path = msg;

        // prepend a backslash if input is a UNC path that only starts with one
        if (msg.StartsWith(@"\") && !msg.StartsWith(@"\\")) {
            path = Helper.UNCPath("\\" + msg);
        } // if
        else {
            path = Helper.UNCPath(msg);
        } // else

        // execute command with full permissions
        await _cmdHandler.ExecuteExternal(ctx.Guild.Id, ctx.Channel.Id, ".localplaylist " + path);
    }

    /// <summary>
    /// Hacky alias of NetworkPlay
    /// </summary>
    [cmd]
    public async Task npl(GuildContext ctx, [leftover] string msg)
    {
        await NetworkPlay(ctx, msg);
    }

}
