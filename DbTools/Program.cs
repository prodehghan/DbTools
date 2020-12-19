using System;
using System.Data.SqlClient;
using System.IO;
using CommandLine;

namespace DbTools
{
    class Program
    {
        static void Main(string[] args)
        {
            var parsed = Parser.Default.ParseArguments<RestoreOptions, object>(args);
            parsed.WithParsed<RestoreOptions>(Restore);
        }

        static void Restore(RestoreOptions options)
        {
            string auth;
            if (options.User != null)
                auth = $"user={options.User};password={options.Password}";
            else
                auth = "integrated security=SSPI";
            var connection = new SqlConnection($"server={options.Server};database=master;{auth}");
            new DbOperations(connection)
                .RestoreRahkaranDatabase(options.BackupFile, options.Database, options.OutputFolder);
        }
    }
}
