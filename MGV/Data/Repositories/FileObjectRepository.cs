using MGV.Entities;
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

        private readonly SqliteConnection _connectionString;
        private readonly DatabaseInterface _databaseInterface;
        private readonly ILogger _logger;
        private bool _isDisposed = false;

        #endregion Private Fields

        #region Public Constructors

        public FileObjectRepository(SqliteConnection connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
            _databaseInterface = DatabaseInterface.GetInstance(_logger);
        }

        #endregion Public Constructors

        #region Public Methods

        public void AddFileToObject(BaseEntity item, int fileId)
        {
            bool result = _databaseInterface.ExecuteCustomQuery(
                    "INSERT INTO Files_Objects(FileId, ObjectId, ObjectType)" +
                    "Values(@fileId, @objectId, @objectType)",
                    _connectionString,
                    new SqliteParameter { ParameterName = "@fileId", DbType = DbType.Int32, Value = fileId },
                    new SqliteParameter { ParameterName = "@objectId", DbType = DbType.Int32, Value = item.Id },
                    new SqliteParameter { ParameterName = "@objectType", DbType = DbType.Int32, Value = ObjectTypeProvider.For(item.GetType()) }
                );
            if (!result)
            {
                _logger.LogError($"File not added: {item.Name}, {fileId}");
            }
        }

        public void DeleteAllFilesFromObject<T>(int objectId)
        {
            bool result = _databaseInterface.ExecuteCustomQuery(
                    "Delete From Files_Objects " +
                    "Where ObjectId = @objectId and ObjectType = @objectType",
                    _connectionString,
                    new SqliteParameter { ParameterName = "@ObjectId", DbType = DbType.Int32, Value = objectId },
                    new SqliteParameter { ParameterName = "@objectType", DbType = DbType.Int32, Value = ObjectTypeProvider.For(typeof(T)) }
                );
            if (!result)
            {
                _logger.LogError($"FileObject not removed: {objectId}, {typeof(T)}");
            }
        }

        public void DeleteFileFromObject(BaseEntity item, int fileId)
        {
            bool result = _databaseInterface.ExecuteCustomQuery(
                    "Delete From Files_Objects " +
                    "Where ObjectId = @objectId and ObjectType = @objectType and FileId = @fileId",
                    _connectionString,
                    new SqliteParameter { ParameterName = "@ObjectId", DbType = DbType.Int32, Value = item.Id },
                    new SqliteParameter { ParameterName = "@objectType", DbType = DbType.Int32, Value = ObjectTypeProvider.For(item.GetType()) },
                    new SqliteParameter { ParameterName = "@fileId", DbType = DbType.Int32, Value = fileId }
                );
            if (!result)
            {
                _logger.LogError($"FileObject not removed: {item.Id},{fileId} ,{item.GetType()}");
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

            result = _databaseInterface.GetSimpleDataList<int>(
                    "Select Files_Objects.FileId From Files_Objects " +
                    "Where ObjectId = @objectId And ObjectType = @objectType",
                    "FileId",
                    _connectionString,
                    new SqliteParameter { ParameterName = "@objectId", DbType = DbType.Int32, Value = item.Id },
                    new SqliteParameter { ParameterName = "@objectType", DbType = DbType.Int32, Value = ObjectTypeProvider.For(item.GetType()) }
                );

            return result;
        }

        public IEnumerable<File> GetFiles(BaseEntity item)
        {
            IEnumerable<int> filesIds;
            IEnumerable<File> result;

            filesIds = GetAllFileIdForObject(item);
            using (var fileRepo = new FileRepository(_connectionString, _logger))
            {
                result = filesIds.Select(fileId => fileRepo.Get(fileId));
            }

            return result;
        }

        public void UpdateFilesForObject(BaseEntity newItem, BaseEntity oldItem)
        {
            foreach (var newFile in newItem.Files.Except(oldItem.Files))
            {
                AddFileToObject(newItem, newFile.Id);
            }
            foreach (var oldFile in oldItem.Files.Except(newItem.Files))
            {
                DeleteFileFromObject(newItem, oldFile.Id);
            }
        }

        #endregion Public Methods
    }
}