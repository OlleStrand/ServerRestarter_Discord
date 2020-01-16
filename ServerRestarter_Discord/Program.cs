using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Discord.Commands;
using System.Windows.Forms;


namespace ServerRestarter_Discord
{
    class ServerInfo
    {

    }

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());

        }
    }

    public class Commands : ModuleBase<SocketCommandContext>
    {

    }
}
