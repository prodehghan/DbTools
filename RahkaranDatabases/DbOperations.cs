using PetaPoco;
using RahkaranDatabases.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace RahkaranDatabases
{
    public class DbOperations : IDisposable
    {
        IDbConnection _connection;
        private bool _disposedValue;

        public DbOperations(IDbConnection connection)
        {
            _connection = connection;
        }

        private void OpenConnection()
        {
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
        }

        private Database GetDatabase()
        {
            OpenConnection();
            return new Database(_connection) { EnableAutoSelect = false };
        }

        public IReadOnlyCollection<DatabaseFile> GetDatabaseFileListFromBackup(string backupPath)
        {
            return GetDatabase().Query<DatabaseFile>(
                "RESTORE FILELISTONLY FROM DISK = @backupPath",
                new { backupPath }).ToList();
        }

        public void RestoreRahkaranDatabase(string backupFile, string toDatabaseName, string toOutputFolder)
        {
            var databaseFileList = GetDatabaseFileListFromBackup(backupFile);
            int paramIndex = 1;
            var cmd = $"RESTORE DATABASE [{toDatabaseName}] FROM DISK = @0 WITH FILE = 1,\r\n" +
                string.Join(",\r\n", databaseFileList.Select(df => $"MOVE @{paramIndex++} TO @{paramIndex++}"));
            var args =  databaseFileList
                .SelectMany(df =>
                    new string[]
                    {
                        df.LogicalName,
                        Path.Combine(toOutputFolder, toDatabaseName + GetSuffix(df.FileGroupName))
                    })
                .Prepend(backupFile);

            KillConnections(toDatabaseName);
            GetDatabase().Execute(cmd, args.ToArray());
        }

        public void KillConnections(string dbName)
        {
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
