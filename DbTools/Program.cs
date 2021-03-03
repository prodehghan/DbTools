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
                var parser = new Parser(config =>
                {
                    config.HelpWriter = Console.Error;
                    config.CaseInsensitiveEnumValues = true;
                });
                var parsed = parser.ParseArguments<RestoreOptions, BackupOptions>(args);
                parsed.WithParsed<RestoreOptions>(o => Restore(o, log));
                parsed.WithParsed<BackupOptions>(o => Backup(o, log));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Something went wrong.");
            }

            Log.CloseAndFlush();
        }

        private static void Backup(BackupOptions options, ILogger logger)
        {
            var connection = CreateConnection(options);
            new DbOperations(connection, logger)
                .BackupDatabase(options.Database, options.ToFile, options.ParsedCompression, options.Type);

            logger.Information("Done.");
        }

        static void Restore(RestoreOptions options, ILogger logger)
        {
            var connection = CreateConnection(options);
            new DbOperations(connection, logger)
                .RestoreDatabase(options.BackupFile, options.Database, options.OutputFolder);

            logger.Information("Done.");
        }

        private static SqlConnection CreateConnection(DbOptionsBase options)
        {
            string auth;
            if (options.User != null)
                auth = $"user={options.User};password={options.Password}";
            else
                auth = "integrated security=SSPI";
            return new SqlConnection($"server={options.Server};database=master;{auth}");
        }
    }
}
