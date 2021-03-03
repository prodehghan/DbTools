using CommandLine;

namespace DbTools
{
    [Verb("backup", HelpText = "Backups a database to a file.")]
    public class BackupOptions : DbOptionsBase
    {
        public BackupOptions()
        {
        }

        public BackupOptions(string server, string user, string password, string database, string backupFile, string compression, BackupType type)
            : base(server, user, password)
        {
            Database = database;
            BackupFile = backupFile;
            Compression = compression;
            Type = type;
        }

        [Option('d', Required = true, HelpText = "The name of the database to backup.")]
        public string Database { get; set; }
        [Option('b', Required = true, HelpText = "Path to the backup file to create or append to.")]
        public string BackupFile { get; set; }
        [Option('c', Required = false, HelpText = "Compression settings - default: server settings, 'y' or 'yes' or '1': compress, 'n' or 'no' or '0': do not compress")]
        public string Compression { get; set; }
        [Option('t', Required = false, Default = BackupType.CopyOnly, HelpText = "Backup type - 'copyOnly' (default), 'full', 'differential'")]
        public BackupType Type { get; set; }
    }

    public enum BackupType
    {
        CopyOnly,
        Full,
        Differential
    }
}
