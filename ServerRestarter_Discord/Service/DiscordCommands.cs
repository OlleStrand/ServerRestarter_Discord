using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerRestarter_Discord
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        public static MainWindow mainWindow;

        [Command("start")]
        [Summary("Start the server")]
        public async Task Start()
        {
            SocketGuildUser user = Context.User as SocketGuildUser;
            if (!user.GuildPermissions.Administrator)
            {
                var msg = await ReplyAsync("You don't have permission to perform this command");
                await Task.Delay(3000);
                await msg.DeleteAsync();

                return;
            }

            await ReplyAsync(mainWindow.StartServer(true));
        }

        [Command("stop")]
        [Summary("Stop the server")]
        public async Task Stop()
        {
            var user = Context.User as SocketGuildUser;
            if (!user.GuildPermissions.Administrator)
            {
                var msg = await ReplyAsync("You don't have permission to perform this command");
                await Task.Delay(3000);
                await msg.DeleteAsync();

                return;
            }

            await ReplyAsync(mainWindow.StopServer(true));
        }

        [Command("restart")]
        [Summary("Restart the server")]
        public async Task Restart()
        {
            var user = Context.User as SocketGuildUser;
            if (!user.GuildPermissions.Administrator)
            {
                var msg = await ReplyAsync("You don't have permission to perform this command");
                await Task.Delay(3000);
                await msg.DeleteAsync();

                return;
            }

            await ReplyAsync(mainWindow.StartServer(true, true));
        }
    }
}
