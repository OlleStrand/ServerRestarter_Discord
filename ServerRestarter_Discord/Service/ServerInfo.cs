using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerRestarter_Discord.Service
{
    class ServerInfo
    {
        public string License { get; protected set; }
        public string Email { get; protected set; }

        public string DiscordToken { get; protected set; }
        public List<DateTime> RestartTimes { get; protected set; }
        public string DefaultPath { get; protected set; }
    }
}
