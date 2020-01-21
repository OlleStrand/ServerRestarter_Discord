using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ServerRestarter_Discord.Service
{
    class Authentication
    {
        public bool IsValid()
        {
            Database database = new Database();
            if (database.IsKeyInDB(ServerInfo.License))
            {
                return true;
            }
            return false;
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

    class Database
    {
        private MySqlConnection _connection;

        public Database(string connVar="Default")
        {
            Initialize(connVar);
        }

        private void Initialize(string connVar)
        {
            string connstring = ConfigurationManager.ConnectionStrings[connVar].ToString();
            _connection = new MySqlConnection(connstring);
        }

        internal bool IsKeyInDB(string license)
        {
            throw new NotImplementedException();
        }
    }
}
