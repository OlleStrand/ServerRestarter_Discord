using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerRestarter_Discord.Service
{
    static class ServerInfo
    {
        static public string License { get; set; }
        static public string Email { get; set; }

        static public string DiscordToken { get; set; }
        static public List<int> RestartHours { get; set; }
        static public List<int> RestartMinutes { get; set; }
        static public string DefaultPath { get; set; }

        static ServerInfo()
        {
            LoadConfig();
        }

        private static void LoadConfig()
        {
            RestartHours = new List<int>();
            RestartMinutes = new List<int>();

            using (var reader = new StreamReader(Path.GetDirectoryName(Application.ExecutablePath) + @"/config.json"))
            {
                string json = reader.ReadToEnd();

                JObject jObject = JObject.Parse(json);
                License = (string)jObject["License"];
                Email = (string)jObject["Email"];
                DiscordToken = (string)jObject["DiscordToken"];
                DefaultPath = (string)jObject["DefaultPath"];

                JToken jRestartTimes = jObject["RestartTimes"];

                foreach (var rs in jRestartTimes)
                {
                    RestartHours.Add(Convert.ToInt32(rs["hour"]));
                    RestartMinutes.Add(Convert.ToInt32(rs["minute"]));
                }
            }
        }
    }
}
