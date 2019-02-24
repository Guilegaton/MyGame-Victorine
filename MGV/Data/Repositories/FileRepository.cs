using Dapper;
using MGV.Entities;
using MGV.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;

namespace MGV.Data.Repositories
{
    public class FileRepository : IRepository<File>
    {
        #region Private Fields

        private IDbConnection _connectionString { get { return _transaction.Connection; } }
        private readonly ILogger _logger;
        private readonly IDbTransaction _transaction;
        private bool _isDisposed = false;

        #endregion Private Fields

        #region Public Constructors

        public FileRepository(IDbTransaction transaction, ILogger logger)
        {
            _logger = logger;
            _transaction = transaction;
        }

        #endregion Public Constructors

        #region Public Methods

        public void Create(File item)
        {
            var sql = "INSERT INTO Files(Name, AsBytes, FileType, Extension)" +
                    "Values(@Name, @AsBytes, @fileType, @Extension)";

            var result = _connectionString.Execute(sql, new { item.FileName, item.AsBytes, item.FileType, item.Extension }, _transaction);

            if (result <= 0)
            {
                _logger.LogError($"File not created: {item.FileName}, {item.Extension}, {item.FileType}");
            }
        }

        public void Delete(int id)
        {
            var sql = "Delete From Files Where Files.FileId = @id";

            var result = _connectionString.Execute(sql, new { id }, _transaction);

            if (result <= 0)
            {
                _logger.LogError($"File not removed: {id}");
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

        public File Get(int id)
        {
            File result;
            var sql = "Select * From Files Where Files.FileId = @id";

            result = _connectionString.QueryFirst<File>(sql, new { id });

            if (result == null)
            {
                _logger.LogError($"File not find: {id}");
            }
            return result;
        }

        public File Get(string name)
        {
            File result = default(File);
            var sql = "Select * From Files Where Files.Name = @name";

            try
            {
                result = _connectionString.QueryFirst<File>(sql, new { name });
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            return result;
        }

        public IEnumerable<File> GetAll()
        {
            var sql = "Select * From Files";

            IEnumerable<File> result = _connectionString.Query<File>(sql);
            
            if (result == null)
            {
                _logger.LogError($"Files not find");
            }
            return result;
        }

        public void Update(File item)
        {
            var sql = "Update Files" +
                    "Set Name = @Name, AsBytes = @AsBytes, FileType = @FileType, Extension = @Extention)" +
                    "Where Id = @Id";

            var result = _connectionString.Execute(sql, new { item.FileName, item.AsBytes, item.FileType, item.Extension, item.FileId }, _transaction);

            if (result <= 0)
            {
                _logger.LogError($"File not updated: {item.FileId}, {item.FileName}, {item.Extension}, {item.FileType}");
            }
        }

        #endregion Public Methods
    }
}