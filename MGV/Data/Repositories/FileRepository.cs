using MGV.Entities;
using MGV.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MGV.Data.Repositories
{
    public class FileRepository : IRepository<File>
    {
        #region Private Fields

        private readonly SqliteConnection _connectionString;
        private readonly DatabaseInterface _databaseInterface;
        private readonly ILogger _logger;
        private bool _isDisposed = false;

        #endregion Private Fields

        #region Public Constructors

        public FileRepository(SqliteConnection connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
            _databaseInterface = DatabaseInterface.GetInstance(_logger);
        }

        #endregion Public Constructors

        #region Public Methods

        public void Create(File item)
        {
            bool result = _databaseInterface.ExecuteCustomQuery(
                    "INSERT INTO Files(Name, AsBytes, FileType, Extension)" +
                    "Values(@name, @asBytes, @fileType, @extension)",
                    _connectionString, new SqliteParameter[] {
                        new SqliteParameter { ParameterName = "@name", DbType = DbType.String, Value = item.Name },
                        new SqliteParameter { ParameterName = "@asBytes", DbType = DbType.Binary, Value = item.AsBytes },
                        new SqliteParameter { ParameterName = "@fileType", DbType = DbType.String, Value = item.FileType },
                        new SqliteParameter { ParameterName = "@extension", DbType = DbType.String, Value = item.Extension }
                    }
                );
            if (!result)
            {
                _logger.LogError($"File not created: {item.Name}, {item.Extension}, {item.FileType}");
            }
        }

        public void Delete(int id)
        {
            bool result = _databaseInterface.ExecuteCustomQuery(
                    "Delete From Files Where Files.Id = @id",
                    _connectionString, new SqliteParameter[] {
                        new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = id }
                    }
                );
            if (!result)
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

            result = _databaseInterface.ExecuteCustomQuery<File>(
                    "Select * From Files Where Files.Id = @id",
                    _connectionString,
                    new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = id }
                ).FirstOrDefault();
            if (result == null)
            {
                _logger.LogError($"File not find: {id}");
            }
            return result;
        }

        public File Get(string name)
        {
            File result = default(File);

            try
            {
                result = _databaseInterface.ExecuteCustomQuery<File>(
                        "Select * From Files Where Files.Name = @name",
                        _connectionString,
                        new SqliteParameter { ParameterName = "@name", DbType = DbType.String, Value = name }
                    ).Single();
            }
            catch(Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            return result;
        }

        public IEnumerable<File> GetAll()
        {
            IEnumerable<File> result;

            result = _databaseInterface.ExecuteCustomQuery<File>(
                    "Select * From Files",
                    _connectionString
                );
            if (result == null)
            {
                _logger.LogError($"Files not find");
            }
            return result;
        }

        public void Update(File item)
        {
            bool result = _databaseInterface.ExecuteCustomQuery(
                    "Update Files" +
                    "Set Name = @name, AsBytes = @asBytes, FileType = @fileType, Extension = @extention)" +
                    "Where Id = @id",
                    _connectionString, new SqliteParameter[] {
                        new SqliteParameter { ParameterName = "@name", DbType = DbType.String, Value = item.Name },
                        new SqliteParameter { ParameterName = "@asBytes", DbType = DbType.Binary, Value = item.AsBytes },
                        new SqliteParameter { ParameterName = "@fileType", DbType = DbType.String, Value = item.FileType },
                        new SqliteParameter { ParameterName = "@extention", DbType = DbType.String, Value = item.Extension },
                        new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = item.Id }
                    }
                );
            if (!result)
            {
                _logger.LogError($"File not updated: {item.Id}, {item.Name}, {item.Extension}, {item.FileType}");
            }
        }

        #endregion Public Methods
    }
}