using System;
using System.Data.SqlClient;
using System.IO;
using CommandLine;
using Serilog;

namespace DbTools
{
    class Program
    {
        static int Main(string[] args)
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
                return parser.ParseArguments<RestoreOptions, BackupOptions>(args)
                    .MapResult(
                        (RestoreOptions o) => Restore(o, log),
                        (BackupOptions o) => Backup(o, log),
                        err => 1
                    );
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Something went wrong.");
                return 2;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static int Backup(BackupOptions options, ILogger logger)
        {
            var connection = CreateConnection(options);
            new DbOperations(connection, logger)
                .BackupDatabase(options.Database, options.ToFile, options.ParsedCompression, options.Type);

            logger.Information("Done.");
            return 0;
        }

        static int Restore(RestoreOptions options, ILogger logger)
        {
            var connection = CreateConnection(options);
            new DbOperations(connection, logger)
                .RestoreDatabase(options.BackupFile, options.Database, options.OutputFolder);

            logger.Information("Done.");
            return 0;
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
