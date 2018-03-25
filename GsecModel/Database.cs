using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using Npgsql;
using NpgsqlTypes;

namespace gsec
{
    public static class Database
    {
        const string HOST = "localhost";
        const string PORT = "5432";
        const string USER = "gsecu";
        const string PASS = "tester";
        const string DB = "gsecdb";
        static string CNX_STR = String.Format("Server={0};Port={1};Username={2};Password={3};Database={4}", HOST, PORT, USER, PASS, DB);

        public static NpgsqlConnection Connection;

        public static bool Connect()
        {
            try
            {
                Connection = new NpgsqlConnection(CNX_STR);
                Connection.Open();
                return true;
            }
            catch (Exception e)
            {
                throw new GsecException("Cannot connect to database", e);
            }
        }

        public static void Disconnect()
        {
            try
            {
                Connection.Close();
            }
            catch (Exception e)
            {
                throw new GsecException("Cannot disconnect from database", e);
            }
        }

        public static int ExecuteSqlCommand(string command)
        {
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand(command, Database.Connection);
                int result = cmd.ExecuteNonQuery();
                return result;
            }
            catch (Exception e)
            {
                return -1;
            }
        }
    }
}
