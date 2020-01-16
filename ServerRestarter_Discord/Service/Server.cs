using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;

namespace ServerRestarter_Discord
{
    class Server
    {
        public event EventHandler<SpecialEvent> LogText;
        protected virtual void OnRequestLogUpdated(SpecialEvent e) => LogText?.Invoke(this, e);

        readonly List<int> _restartHours = new List<int> { 1, 9, 17 };

        public bool IsRunning = false;
        private bool _restarted = false;
        public Process StartServer(string batFilePath)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = "cmd.exe";

            proc.StartInfo.Arguments = batFilePath != "" ? "/c " + batFilePath : "/c pause";

            Log("Starting Server...");

            proc.Start();
            IsRunning = true;
            Log("Server Started");
            Task.Run(() => IsServerRunning(proc, batFilePath));

            return proc;
        }

        private void IsServerRunning(Process proc, string batFilePath)
        {
            Process[] processes;
            for (; ; )
            {
                processes = Process.GetProcessesByName(proc.ProcessName ?? "cmd");
                bool _procIsRunning = false;
                foreach (Process process in processes)
                {
                    if (process.Id == proc.Id)
                        _procIsRunning = true;
                }

                if ((processes.Length == 0 || !_procIsRunning) && IsRunning)
                {
                    Log("Process not found, restarting");

                    IsRunning = false;

                    MainWindow._SPID = StartServer(batFilePath);
                    break;
                }
                if (ShouldRestart(proc, batFilePath))
                    break;
                
                Log($"Process {proc.ProcessName}[{proc.Id}] is running");
                Thread.Sleep(1000);
            }
        }

        private bool ShouldRestart(Process proc, string batFilePath)
        {
            if (_restartHours.Contains(Convert.ToInt32(DateTime.Now.Hour)) && DateTime.Now.Minute == 0 && IsRunning && !_restarted)
            {
                IsRunning = false;
                Log("Restarting Server");
                StopServer(proc);
                Thread.Sleep(500);

                Process proc0 = StartServer(batFilePath);
                MainWindow._SPID = proc0;

                _restarted = true;
                Thread.Sleep(10000 * 60 * 2);
                _restarted = false;

                Log("Restart Phase Done");
                return true;
            }
            return false;
        }

        public void StopServer(Process proc)
        {
            Log("Stopping Server...");
            IsRunning = false;
            try
            {
                KillProcessAndChildrens(proc.Id);
                Log("Server Stopped");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Seriously?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Log($"Stop failed: {ex.Message}");
            }
        }

        private void KillProcessAndChildrens(int pid)
        {
            ManagementObjectSearcher processSearcher = new ManagementObjectSearcher
              ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection processCollection = processSearcher.Get();

            // We must kill child processes first!
            if (processCollection != null)
            {
                foreach (ManagementObject mo in processCollection)
                {
                    Log($"Ending Process: {Convert.ToInt32(mo["ProcessID"])}");
                    KillProcessAndChildrens(Convert.ToInt32(mo["ProcessID"])); //kill child processes(also kills childrens of childrens etc.)
                }
            }

            // Then kill parents.
            try
            {
                Process proc = Process.GetProcessById(pid);
                if (!proc.HasExited) proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }

        private void Log(string text)
        {
            SpecialEvent e = new SpecialEvent(text);
            OnRequestLogUpdated(e);
        }
    }
}
