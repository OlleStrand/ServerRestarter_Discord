using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerRestarter_LicenseServer
{
    class DBConnect
    {
         private MySqlConnection _connection;

        public DBConnect(string connVar)
        {
            Initialize(connVar);
        }

        private void Initialize(string connVar)
        {
            string connstring = ConfigurationManager.ConnectionStrings[connVar].ToString();
            _connection = new MySqlConnection(connstring);
        }

        private bool OpenConnection()
        {
            try
            {
                _connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        // Cannot connect to server.
                        break;

                    case 1045:
                        // Invalid user name and/or password.
                        break;
                }
                return false;
            }
        }

        private bool CloseConnection()
        {
            try
            {
                _connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                //ex.Message
                return false;
            }
        }

        public bool IsKeyInDB(string key, string email, string hwid)
        {
            string query = $"SELECT * FROM licenses WHERE serialKey='{key}' AND email='{email}'";

            if (OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, _connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    if (dataReader["hwid"].ToString() == "")
                    {
                        dataReader.Close();
                        CloseConnection();
                        UpdateHWID(email, hwid);
                    }
                    dataReader.Close();
                    CloseConnection();
                    return true;
                }
                dataReader.Close();
                CloseConnection();
                return false;
            }
            else
                return false;
        }

        public bool IsKeyValid(string key, string hwid)
        {
            string query = $"SELECT * FROM licenses WHERE serialKey='{key}' AND hwid='{hwid}'";

            if (OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, _connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    dataReader.Close();
                    CloseConnection();
                    return true;
                }
                dataReader.Close();
                CloseConnection();
                return false;
            }
            else
                return false;
        }

        public void UpdateHWID(string email, string hwid)
        {
            string query = $"UPDATE licenses SET hwid='{hwid}' WHERE email='{email}'";

            if (OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, _connection);

                cmd.ExecuteNonQuery();

                CloseConnection();
            }
        }
    }
}
