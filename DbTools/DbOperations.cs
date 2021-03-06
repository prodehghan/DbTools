using PetaPoco;
using DbTools.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Serilog;

namespace DbTools
{
    public class DbOperations : IDisposable
    {
        IDbConnection _connection;
        private readonly ILogger _logger;
        private bool _disposedValue;
        private Database _database;

        public DbOperations(IDbConnection connection, ILogger logger)
        {
            _connection = connection;
            _logger = logger;
        }

        private void OpenConnection()
        {
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
        }

        private Database GetDatabase()
        {
            OpenConnection();
            return _database ??= new Database(_connection) { EnableAutoSelect = false };
        }

        public IReadOnlyCollection<DatabaseFile> GetDatabaseFileListFromBackup(string backupPath)
        {
            return GetDatabase().Query<DatabaseFile>(
                "RESTORE FILELISTONLY FROM DISK = @backupPath",
                new { backupPath }).ToList();
        }

        public void RestoreDatabase(string backupFile, string databaseName, string outputFolder)
        {
            var databaseFileList = GetDatabaseFileListFromBackup(backupFile);
            int paramIndex = 1;
            var cmd = $"RESTORE DATABASE [{databaseName}] FROM DISK = @0 WITH FILE = 1,\r\n" +
                string.Join(",\r\n", databaseFileList.Select(df => $"MOVE @{paramIndex++} TO @{paramIndex++}"));
            var args = databaseFileList
                .SelectMany(df =>
                    new string[]
                    {
                        df.LogicalName,
                        Path.Combine(outputFolder, databaseName + GetSuffix(df.FileGroupName))
                    })
                .Prepend(backupFile);

            KillConnections(databaseName);

            _logger.Information("Restoring\r\n  Backup file: {backupFile}\r\n" +
                "  Database: {dbName}\r\n  Database files put in: {folder}",
                backupFile, databaseName, outputFolder);

            GetDatabase().Execute(cmd, args.ToArray());
        }

        public void BackupDatabase(string databaseName, string backupFilePath, bool? compress, BackupType backupType)
        {
            List<string> withOptions = new List<string>();
            switch(backupType)
            {
                case BackupType.Full:
                    break;
                case BackupType.Differential:
                    withOptions.Add("DIFFERENTIAL");
                    break;
                case BackupType.CopyOnly:
                    withOptions.Add("COPY_ONLY");
                    break;
            }
            if (compress != null)
                withOptions.Add((compress.Value ? "" : "NO_") + "COMPRESSION");

            var cmd = $"BACKUP DATABASE [{databaseName}] TO DISK = @0";
            if(withOptions.Count > 0)
            {
                cmd += "\r\nWITH " + string.Join(", ", withOptions);
            }

            _logger.Information("Backup\r\n  Database: {databaseName}\r\n" +
                "  To file: {toFile}\r\n  Compress: {compress}\r\n  Type: {type}",
                databaseName, backupFilePath,
                compress.HasValue ? (compress.Value ? "Yes" : "No") : "Default",
                backupType);

            GetDatabase().Execute(cmd, backupFilePath);
        }

        public void KillConnections(string dbName)
        {
            _logger.Information("Killing existing connections to '{dbName}'.", dbName);

            var cmd = @"
DECLARE @kill varchar(8000) = '';  
SELECT @kill = @kill + 'kill ' + CONVERT(varchar(5), session_id) + ';'  
FROM sys.dm_exec_sessions
WHERE database_id  = db_id(@0)

EXEC(@kill);";
            var db = GetDatabase();
            db.EnableNamedParams = false;
            db.Execute(cmd, dbName);
        }

        private string GetSuffix(string fileGroup)
        {
            if (fileGroup == "PRIMARY")
                return ".mdf";
            if (fileGroup == null)
                return "_log.ldf";
            return "_" + fileGroup + ".ndf";
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _connection.Close();
                _disposedValue = true;
            }
        }

        ~DbOperations()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
