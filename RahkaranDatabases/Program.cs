using System;
using System.Data.SqlClient;
using System.IO;

namespace RahkaranDatabases
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = ".";
            var connection = new SqlConnection($"server={server};database=master;integrated security=SSPI");
            var backupFile = @"D:\SGIBS\Fixtures\DB0-Operations.bak";
            var database = "DB0-Operations";
            var dbFolder = @"D:\SGIBS\Data";
            new DbOperations(connection).RestoreRahkaranDatabase(backupFile, database, dbFolder);
        }
    }
}
