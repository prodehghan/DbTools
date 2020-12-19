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

        public IReadOnlyCollection<DatabaseFile> GetDatabaseFileListFromBackup(string backupPath)
        {
            OpenConnection();

            var db = new Database(_connection);
            db.EnableAutoSelect = false;
            return db.Query<DatabaseFile>(
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

            OpenConnection();
            int result = new Database(_connection).Execute(cmd, args.ToArray());
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
