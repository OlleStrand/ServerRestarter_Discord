using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Management;
using System.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerRestarter_Discord.Service
{
    class Authentication
    {
        public static bool IsValid()
        {
            string receieveQueue = RandomString(5);
            string MainPath = @"FormatName:Direct=TCP:173.249.11.2\private$\";

            using (MessageQueue input = new MessageQueue(MainPath + "MainQueue", QueueAccessMode.Send)
            {
                Formatter = new ActiveXMessageFormatter()
            })
            {
                using (MessageQueue output = new MessageQueue(MainPath+receieveQueue, QueueAccessMode.Receive)
                {
                    MessageReadPropertyFilter = { Id = true, CorrelationId = true, Body = true, Label = true },
                    Formatter = new ActiveXMessageFormatter()
                })
                {
                    Message msg = new Message
                    {
                        Body = $"{ServerInfo.Email}|{ServerInfo.License}|{GetHWID().Result.ToString()}|{receieveQueue}",
                        TimeToReachQueue = new TimeSpan(0, 0, 20),
                        TimeToBeReceived = new TimeSpan(0, 0, 40)
                    };

                    try
                    {
                        input.Send(msg);
                        var id = msg.Id;

                        //TODO SEND BACK MESSAGE
                        try
                        {
                            bool messageReceied = false;
                            Message resultMessage = null;
                            while (!messageReceied)
                            {
                                resultMessage  = output.ReceiveByCorrelationId(id);
                                if (resultMessage != null)
                                    messageReceied = true;
                            }

                            string label = resultMessage.Label;
                            bool result = (bool)resultMessage.Body;

                            if (!string.IsNullOrEmpty(label) && !label.Contains("ERROR") && result)
                                return true;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                    catch (MessageQueueException)
                    {
                        //Console.WriteLine(mqx.ToString());
                        return false;
                    }
                }
            }

            return false;
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static async Task<string> GetHWID()
        {
            byte[] bytes;
            byte[] hashedBytes;
            StringBuilder sb = new StringBuilder();

            Task t1 = Task.Run(() =>
            {
                sb.Append(HWID.ProcessorID());
                sb.Append(HWID.DiskID());
                sb.Append(HWID.MotherBoardID());
            });
            Task.WaitAll(t1);
            bytes = Encoding.UTF8.GetBytes(sb.ToString());
            hashedBytes = SHA256.Create().ComputeHash(bytes);

            return await Task.FromResult(Convert.ToBase64String(hashedBytes));
        }

        private class HWID
        {
            public static string ProcessorID()
            {
                ManagementObjectCollection mbsList = null;
                ManagementObjectSearcher mbs = new ManagementObjectSearcher("Select * From Win32_processor");
                mbsList = mbs.Get();
                string id = "";
                foreach (ManagementObject mo in mbsList)
                {
                    id = mo["ProcessorID"].ToString();
                }
                return id;
            }

            public static string DiskID()
            {
                ManagementObject dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""c:""");
                dsk.Get();
                return dsk["VolumeSerialNumber"].ToString();
            }

            public static string MotherBoardID()
            {
                ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
                ManagementObjectCollection moc = mos.Get();
                string serial = "";
                foreach (ManagementObject mo in moc)
                {
                    serial = (string)mo["SerialNumber"];
                }
                return serial;
            }
        }

        public sealed class PasswordHash
        {
            private const int SaltSize = 16, HashSize = 20, HashIter = 10000;
            private readonly byte[] _salt, _hash;

            public PasswordHash(string password)
            {
                new RNGCryptoServiceProvider().GetBytes(_salt = new byte[SaltSize]);
                _hash = new Rfc2898DeriveBytes(password, _salt, HashIter).GetBytes(HashSize);
            }

            public PasswordHash(byte[] hashBytes)
            {
                Array.Copy(hashBytes, 0, _salt = new byte[SaltSize], 0, SaltSize);
                Array.Copy(hashBytes, SaltSize, _hash = new byte[HashSize], 0, HashSize);
            }

            public PasswordHash(byte[] salt, byte[] hash)
            {
                Array.Copy(salt, 0, _salt = new byte[SaltSize], 0, SaltSize);
                Array.Copy(hash, 0, _hash = new byte[HashSize], 0, HashSize);
            }

            public byte[] ToArray()
            {
                byte[] hashBytes = new byte[SaltSize + HashSize];
                Array.Copy(_salt, 0, hashBytes, 0, SaltSize);
                Array.Copy(_hash, 0, hashBytes, SaltSize, HashSize);
                return hashBytes;
            }

            public byte[] Salt { get { return (byte[])_salt.Clone(); } }
            public byte[] Hash { get { return (byte[])_hash.Clone(); } }

            public bool Verify(string password)
            {
                byte[] test = new Rfc2898DeriveBytes(password, _salt, HashIter).GetBytes(HashSize);
                for (int i = 0; i < HashSize; i++)
                    if (test[i] != _hash[i])
                        return false;
                return true;
            }
        }
    }
}
