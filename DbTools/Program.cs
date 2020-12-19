using System;
using System.Data.SqlClient;
using System.IO;
using CommandLine;
using Serilog;

namespace DbTools
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger = log;

            try
            {
                var parsed = Parser.Default.ParseArguments<RestoreOptions, object>(args);
                parsed.WithParsed<RestoreOptions>(o => Restore(o, log));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Something went wrong.");
            }

            Log.CloseAndFlush();
        }

        static void Restore(RestoreOptions options, ILogger logger)
        {
            string auth;
            if (options.User != null)
                auth = $"user={options.User};password={options.Password}";
            else
                auth = "integrated security=SSPI";
            var connection = new SqlConnection($"server={options.Server};database=master;{auth}");
            new DbOperations(connection, logger)
                .RestoreRahkaranDatabase(options.BackupFile, options.Database, options.OutputFolder);

            logger.Information("Done.");
        }
    }
}
