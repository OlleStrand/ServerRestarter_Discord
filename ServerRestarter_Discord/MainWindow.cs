using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ServerRestarter_Discord.Service;
using System.Collections;
using System.Threading;

namespace ServerRestarter_Discord
{
    public partial class MainWindow : Form
    {
        static List<DateTime> _restartTimes = new List<DateTime> {};
        static DateTime restartTime = new DateTime();

        public static Process _SPID;
        private static Server _server;

        private static bool _updateTimer = false;
        System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();

        public MainWindow()
        {
            Thread t = new Thread(new ThreadStart(SplashScreen));
            t.Start();

            MSMQInstaller msmq = new MSMQInstaller();
            msmq.Install(new Hashtable());

            if (!Authentication.IsValid())
            {
                MessageBox.Show("Your license is inactive. Contact Olle_#1634 on Discord", "License not Valid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                t.Abort();
                Close();
            }

            for (int i = 0; i < ServerInfo.RestartHours.Count; i++)
            {
                if (ServerInfo.RestartHours[i] <= DateTime.Now.Hour && ServerInfo.RestartMinutes[i] <= DateTime.Now.Minute)
                    _restartTimes.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1, ServerInfo.RestartHours[i], ServerInfo.RestartMinutes[i], 0));
                else
                    _restartTimes.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, ServerInfo.RestartHours[i], ServerInfo.RestartMinutes[i], 0));
            }

            InitializeComponent();
            Commands.mainWindow = this;
            t.Abort();

            textBoxPath.Text = ServerInfo.DefaultPath;
            _server = new Server();

            logBox.ReadOnly = true;
            logBox.Text += 
                "1. Click browse and find the cmd/bat file for the server.\n" +
                "2. Press Start and the program will handle everything else\n" +
                "3. Press Start Discord Bot to use discord commands(setup required)\n";

            UpdateTitleText += new EventHandler<SpecialEvent>(UpdateTitle);
        }

        private void SplashScreen()
        {
            try { Application.Run(new SplashScreen()); }
            catch (Exception) {  }
            
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\\ServerLog " + DateTime.Today.ToString("dd-MM-yyyy") + ".txt";

            StreamWriter sw = new StreamWriter(File.Open(path, FileMode.Append));

            for (int i = 0; i <= logBox.Lines.Length - 1; i++)
            {
                sw.WriteLine(logBox.Lines[i] + "\n");
            }
            sw.Close();
        }


        public string StartServer(bool isDiscord = false, bool restart = false)
        {
            if (_server.IsRunning)
            {
                if (isDiscord)
                {
                    if (!restart)
                        return "Server is already running";

                    //Stop server part
                    _updateTimer = false;
                    _server.StopServer(_SPID);
                    _server.LogText -= new EventHandler<SpecialEvent>(Server_Log);
                    _server = new Server();
                    UpdateTitle("ASR | Stopped");

                    //Start server part
                    _server.LogText += new EventHandler<SpecialEvent>(Server_Log);
                    _SPID = _server.StartServer(textBoxPath.Text);
                    UpdateTitle("ASR | Started");
                    _updateTimer = true;
                    BeginInvoke(new Action(
                        () => TimerUpdate()
                    ));

                    return "Restarted Server";
                } else
                {
                    DialogResult dialogResult = MessageBox.Show("Server is already running. Do you want to restart it?", "Running Server",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Stop);
                    if (dialogResult == DialogResult.OK || restart)
                    {
                        //Stop server part
                        _updateTimer = false;
                        _server.StopServer(_SPID);
                        _server.LogText -= new EventHandler<SpecialEvent>(Server_Log);
                        _server = new Server();
                        UpdateTitle("ASR | Stopped");

                        //Start server part
                        _server.LogText += new EventHandler<SpecialEvent>(Server_Log);
                        _SPID = _server.StartServer(textBoxPath.Text);
                        UpdateTitle("ASR | Started");
                        _updateTimer = true;
                        BeginInvoke(new Action(
                            () => TimerUpdate()
                        ));

                        return "";
                    }
                }
            } else
            {
                _SPID = _server.StartServer(textBoxPath.Text);
                UpdateTitle("ASR | Started");
                _updateTimer = true;
                BeginInvoke(new Action(
                    () => TimerUpdate()
                ));

                return "Started server";
            }
            return "?";
        }

        public string StopServer(bool isDiscord = false)
        {
            if (!_server.IsRunning)
                return "Server is not running";

            if (isDiscord)
            {
                _updateTimer = false;
                _server.StopServer(_SPID);
                _server.LogText -= new EventHandler<SpecialEvent>(Server_Log);
                _server = new Server();
                UpdateTitle("ASR | Stopped");

                return "Server stopped";
            } else
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to stop the server?", "Stop server",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (dialogResult == DialogResult.Yes || isDiscord)
                {
                    _updateTimer = false;
                    _server.StopServer(_SPID);
                    _server.LogText -= new EventHandler<SpecialEvent>(Server_Log);
                    _server = new Server();
                    UpdateTitle("ASR | Stopped");

                    return "Server stopped";
                }
            }

            return "?";
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            StartServer();
        }

        private void ButtonStop_Click(object sender, EventArgs e)
        {
            StopServer();
        }

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog {
                Title = "Browse To Server File",
                DefaultExt = "cmd",
                Filter = "CMD files (*.cmd)|*.cmd|BAT files (*.bat)|*.bat|All files (*.*)|*.*"
            };

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                textBoxPath.Text = openFile.FileName;
            }
        }

        DateTime GetClosestTime()
        {
            long min = long.MaxValue;
            long diff;
            foreach (DateTime rt in _restartTimes)
            {
                if (DateTime.Now < rt)
                {
                    diff = Math.Abs(DateTime.Now.Ticks - rt.Ticks);
                    if (diff < min)
                    {
                        min = diff;
                        restartTime = rt;
                    }
                }
            }
            return restartTime;
        }

        void TimerUpdate()
        {
            _timer.Interval = 500;
            _timer.Tick += new EventHandler(Tick);
            TimeSpan ts = GetClosestTime().Subtract(DateTime.Now);
            labelTime.Text = ts.ToString("'Restart in 'h' Hours, 'm' Minutes and 's' Seconds'");
            _timer.Start();
        }

        void Tick(object sender, EventArgs e)
        {
            if (_updateTimer)
            {
                _restartTimes.Clear();
                for (int i = 0; i < ServerInfo.RestartHours.Count; i++)
                {
                    if (ServerInfo.RestartHours[i] <= DateTime.Now.Hour && ServerInfo.RestartMinutes[i] <= DateTime.Now.Minute)
                        _restartTimes.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day+1, ServerInfo.RestartHours[i], ServerInfo.RestartMinutes[i], 0));
                    else
                        _restartTimes.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, ServerInfo.RestartHours[i], ServerInfo.RestartMinutes[i], 0));
                }

                TimeSpan ts = GetClosestTime().Subtract(DateTime.Now);
                labelTime.Text = ts.ToString("'Restart in 'h' Hours, 'm' Minutes and 's' Seconds'");
            }
            else
            {
                labelTime.Text = "Restart in ";
                _timer.Stop();
            }
        }

        //LOGGING METHODS
        private static string _lastMessage = "";
        private static int _messageCount = 1;
        private static DateTime _messageTime = DateTime.Now;
        private void LogWrite(string text)
        {
            if (text == _lastMessage)
            {
                _messageCount++;

                string[] lines = logBox.Lines;
                lines[lines.Length - 2] = $"{_messageTime} - {DateTime.Now} > {text} x{_messageCount}";
                logBox.Lines = lines;
            }
            else
            {
                logBox.Text += $"{DateTime.Now} > {text} \n";
                _messageCount = 1;
                _messageTime = DateTime.Now;
            }
            _lastMessage = text;
        }

        void Server_Log(object sender, SpecialEvent e)
        {
            BeginInvoke(new Action(
            () =>
                {
                    if (e.Text == _lastMessage)
                    {
                        _messageCount++;

                        string[] lines = logBox.Lines;
                        lines[lines.Length-2] = $"{_messageTime} - {DateTime.Now} > {e.Text} x{_messageCount}";
                        logBox.Lines = lines;
                    }
                    else
                    {
                        logBox.Text += $"{DateTime.Now} > {e.Text} \n";
                        _messageCount = 1;
                        _messageTime = DateTime.Now;
                    }
                    _lastMessage = e.Text;
                }
            ));
        }

        void UpdateTitle(object sender, SpecialEvent e)
        {
            BeginInvoke(new Action(
                () => this.Text = e.Text 
            ));
        }

        //Automatic scrolling
        private void LogBox_TextChanged(object sender, EventArgs e)
        {
            logBox.SelectionStart = logBox.Text.Length;
            logBox.ScrollToCaret();
        }

        //Discord related stuff
        private DiscordSocketClient _client;
        private CommandHandler _commandHandler;

        private async void ButtonDcBot_ClickAsync(object sender, EventArgs e)
        {
            buttonDcBot.Enabled = false;
            _client = new DiscordSocketClient();
            _commandHandler = new CommandHandler();

            LogClient += new EventHandler<SpecialEvent>(Server_Log);
            _client.Log += Client_Log;

            try
            {
                await _client.LoginAsync(TokenType.Bot, ServerInfo.DiscordToken);
                await _client.StartAsync();
                await _commandHandler.InstallCommandsAsync(_client);
            }
            catch (Exception ex)
            {
                LogWrite(ex.ToString());
            }

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        public event EventHandler<SpecialEvent> LogClient;
        private Task Client_Log(LogMessage msg)
        {
            SpecialEvent e = new SpecialEvent(msg.ToString());
            LogClient?.Invoke(this, e);
            return Task.CompletedTask;
        }

        event EventHandler<SpecialEvent> UpdateTitleText;
        private Task UpdateTitle(string title)
        {
            SpecialEvent e = new SpecialEvent(title);
            UpdateTitleText?.Invoke(this, e);
            return Task.CompletedTask;
        }
    }

    public class SpecialEvent : EventArgs
    {
        public SpecialEvent(string text) => Text = text;
        public string Text { get; }
    }
}
