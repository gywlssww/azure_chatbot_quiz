using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace Microsoft.BotBuilderSamples
{
    public class dbtask
    {
        private static string Host = "dbserver502v200722.postgres.database.azure.com";
        private static string User = "ding@dbserver502v200722";
        private static string DBname = "mypgsqldb";
        private static string Password = "Hhj9060!";
        private static string Port = "5432";

        public string connstring =
                String.Format(
                    "Server={0}; User Id={1}; Database={2}; Port={3}; Password={4};SSLMode=Prefer",
                    Host,
                    User,
                    DBname,
                    Port,
                    Password);


        public string getConnString() {
            return connstring;
        }
    }
}
