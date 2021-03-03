using CommandLine;

namespace DbTools
{
    [Verb("restore", HelpText = "Restores a database backup file.")]
    public class RestoreOptions : DbOptionsBase
    {
        public RestoreOptions()
        {
        }

        public RestoreOptions(string server, string user, string password, string backupFile, string database, string outputFolder)
            : base(server, user, password)
        {
            BackupFile = backupFile;
            Database = database;
            OutputFolder = outputFolder;
        }

        [Option('b', Required = true, HelpText = "Path to the backup file to restore")]
        public string BackupFile { get; set; }
        [Option('d', Required = true, HelpText = "The name of the database to create/restore.")]
        public string Database { get; set; }
        [Option('o', Required = true, HelpText = "The path to an existing directory to put the database files.")]
        public string OutputFolder { get; set; }
    }
}
