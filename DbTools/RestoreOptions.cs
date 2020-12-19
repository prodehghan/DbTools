using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbTools
{
    [Verb("restore", isDefault:true, HelpText = "Restores a database backup.")]
    public class RestoreOptions
    {
        [Option('s', Required = true, HelpText = "The SQL Server instance to connect to.")]
        public string Server { get; }
        [Option('u', Required = false, HelpText = "Login user name for the server. If omitted, Windows Authentication is used."]
        public string User { get; }
        [Option('p', Required = false, HelpText = "Login password.")]
        public string Password { get; }
        [Option('b', Required = true, HelpText = "Path to the backup file to restore")]
        public string BackupFile { get; }
    }
}
