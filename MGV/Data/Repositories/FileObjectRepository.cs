using Dapper;
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

        private IDbConnection _connectionString { get { return _transaction.Connection; } }
        private readonly ILogger _logger;
        private readonly IDbTransaction _transaction;
        private bool _isDisposed = false;

        #endregion Private Fields

        #region Public Constructors

        public FileObjectRepository(IDbTransaction transaction, ILogger logger)
        {
            _logger = logger;
            _transaction = transaction;
        }

        #endregion Public Constructors

        #region Public Methods

        public void AddFileToObject(BaseEntity item, int fileId)
        {
            var sql = "INSERT INTO Files_Objects(FileId, ObjectId, ObjectType)" +
                    "Values(@fileId, @objectId, @objectType)";

            var result = _connectionString.Execute(sql, new { fileId, objectId = item.Id, objectType = ObjectTypeProvider.For(item.GetType()) }, _transaction);

            if (result <= 0)
            {
                _logger.LogError($"File not added: {item.Name}, {fileId}");
            }
        }

        public void DeleteAllFilesFromObject<T>(int objectId)
        {
            var sql = "Delete From Files_Objects " +
                    "Where ObjectId = @objectId and ObjectType = @objectType";

            var result = _connectionString.Execute(sql, new { objectId, objectType = ObjectTypeProvider.For(typeof(T)) }, _transaction);

            if (result <= 0)
            {
                _logger.LogError($"FileObject not removed: {objectId}, {typeof(T)}");
            }
        }

        public void DeleteFileFromObject(BaseEntity item, int fileId)
        {
            var sql = "Delete From Files_Objects " +
                    "Where ObjectId = @objectId and ObjectType = @objectType and FileId = @fileId";

            var result = _connectionString.Execute(sql, new { objectId = item.Id, objectType = ObjectTypeProvider.For(item.GetType()), fileId }, _transaction);

            if (result <= 0)
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

        public IEnumerable<File> GetAllFilesForObject(BaseEntity item)
        {
            var sql = "Select Files.* From  Files INNER JOIN Files_Objects ON Files_Objects.FileId = Files.FileId " +
                    "Where Files_Objects.ObjectId = @objectId And Files_Objects.ObjectType = @objectType";

            IEnumerable<File> result = _connectionString.Query<File>(sql, new { objectId = item.Id, objectType = (int)ObjectTypeProvider.For(item.GetType()) });

            return result;
        }

        public void UpdateFilesForObject(BaseEntity newItem, BaseEntity oldItem)
        {
            foreach (var newFile in newItem.Files.Except(oldItem.Files))
            {
                AddFileToObject(newItem, newFile.FileId);
            }
            foreach (var oldFile in oldItem.Files.Except(newItem.Files))
            {
                DeleteFileFromObject(newItem, oldFile.FileId);
            }
        }

        #endregion Public Methods
    }
}