using MGV.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MGV.Data.Repositories
{
    public class FileObjectRepository : IDisposable
    {
        #region Private Fields

        private readonly string _connectionString;
        private readonly DatabaseInterface _databaseInterface;
        private readonly ILogger _logger;
        private bool _isDisposed = false;

        #endregion Private Fields

        #region Public Constructors

        public FileObjectRepository(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
            _databaseInterface = DatabaseInterface.GetInstance(_logger);
        }

        #endregion Public Constructors

        #region Public Methods

        public void AddFileToObject(BaseEntity item, int fileId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                bool result = _databaseInterface.ExecuteCustomQuery(
                        "INSERT INTO Files_Objects(FileId, ObjectId, ObjectType)" +
                        "Values(@fileId, @objectId, @objectType)",
                        connection,
                        new SqliteParameter { ParameterName = "@fileId", DbType = DbType.Int32, Value = fileId },
                        new SqliteParameter { ParameterName = "@objectId", DbType = DbType.Int32, Value = item.Id },
                        new SqliteParameter { ParameterName = "@objectType", DbType = DbType.Int32, Value = ObjectTypeProvider.For(item.GetType()) }
                    );
                if (!result)
                {
                    _logger.LogError($"File not added: {item.Name}, {fileId}");
                }
            }
        }

        public void DeleteAllFilesFromObject<T>(int objectId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                bool result = _databaseInterface.ExecuteCustomQuery(
                        "Delete From Files_Objects " +
                        "Where ObjectId = @objectId and ObjectType = @objectType",
                        connection,
                        new SqliteParameter { ParameterName = "@ObjectId", DbType = DbType.Int32, Value = objectId },
                        new SqliteParameter { ParameterName = "@objectType", DbType = DbType.Int32, Value = ObjectTypeProvider.For(typeof(T)) }
                    );
                if (!result)
                {
                    _logger.LogError($"FileObject not removed: {objectId}, {typeof(T)}");
                }
            }
        }

        public void DeleteFileFromObject(BaseEntity item, int fileId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                bool result = _databaseInterface.ExecuteCustomQuery(
                        "Delete From Files_Objects " +
                        "Where ObjectId = @objectId and ObjectType = @objectType and FileId = @fileId",
                        connection,
                        new SqliteParameter { ParameterName = "@ObjectId", DbType = DbType.Int32, Value = item.Id },
                        new SqliteParameter { ParameterName = "@objectType", DbType = DbType.Int32, Value = ObjectTypeProvider.For(item.GetType()) },
                        new SqliteParameter { ParameterName = "@fileId", DbType = DbType.Int32, Value = fileId }
                    );
                if (!result)
                {
                    _logger.LogError($"FileObject not removed: {item.Id},{fileId} ,{item.GetType()}");
                }
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }

        public IEnumerable<int> GetAllFileIdForObject(BaseEntity item)
        {
            IEnumerable<int> result = Enumerable.Empty<int>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                result = _databaseInterface.GetSimpleDataList<int>(
                        "Select Files_Objects.FileId From Files_Objects" +
                        "Where ObjectId = @id And ObjectType = @objectType",
                        "FileId",
                        connection,
                        new SqliteParameter { ParameterName = "@objectId", DbType = DbType.Int32, Value = item.Id },
                        new SqliteParameter { ParameterName = "@objectType", DbType = DbType.Int32, Value = ObjectTypeProvider.For(item.GetType()) }
                    );
            }

            return result;
        }

        #endregion Public Methods
    }
}