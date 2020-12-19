using CommandLine;

namespace DbTools
{
    [Verb("restore", isDefault: true, HelpText = "Restores a database backup.")]
    public class RestoreOptions
    {
        public RestoreOptions(string server, string user, string password, string backupFile, string database, string outputFolder)
        {
            Server = server;
            User = user;
            Password = password;
            BackupFile = backupFile;
            Database = database;
            OutputFolder = outputFolder;
        }

        [Option('s', Required = true, HelpText = "The SQL Server instance to connect to.")]
        public string Server { get; }
        [Option('u', Required = false, HelpText = "Login user name for the server. If omitted, Windows Authentication is used.")]
        public string User { get; }
        [Option('p', Required = false, HelpText = "Login password.")]
        public string Password { get; }
        [Option('b', Required = true, HelpText = "Path to the backup file to restore")]
        public string BackupFile { get; }
        [Option('d', Required = true, HelpText = "The name of the database to create/restore.")]
        public string Database { get; }
        [Option('o', Required = true, HelpText = "The path to an existing directory to put the database files.")]
        public string OutputFolder { get; }
    }
}
