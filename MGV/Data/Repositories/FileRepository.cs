using MGV.Models;
using MGV.Share;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MGV.Data.Repositories
{
    public class FileRepository : IRepository<File>
    {
        #region Private Fields

        private readonly string _connectionString;
        private readonly DatabaseInterface _databaseInterface;
        private readonly ILogger _logger;

        #endregion Private Fields

        #region Public Constructors

        public FileRepository(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
            _databaseInterface = DatabaseInterface.GetInterface(_logger);
        }

        #endregion Public Constructors

        #region Public Methods

        public void Create(File item)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                bool result = _databaseInterface.ExecuteCustomQuery(
                        "INSERT INTO Files(Name, AsBytes, FileType, Extension)" +
                        "Values(@name, @asBytes, @fileType, @extension)",
                        connection,
                        new SqliteParameter { ParameterName = "@name", DbType = DbType.String, Value = item.Name },
                        new SqliteParameter { ParameterName = "@asBytes", DbType = DbType.Binary, Value = item.AsBytes },
                        new SqliteParameter { ParameterName = "@fileType", DbType = DbType.String, Value = item.FileType },
                        new SqliteParameter { ParameterName = "@extension", DbType = DbType.String, Value = item.Extension }
                    );
                if (!result)
                {
                    _logger.LogError($"Element not created: {item.Name}, {item.Extension}, {item.FileType}");
                }
            }
        }

        public void Delete(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                bool result = _databaseInterface.ExecuteCustomQuery(
                        "Delete From Files Where Files.Id = @id",
                        connection,
                        new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = id }
                    );
                if (!result)
                {
                    _logger.LogError($"Element not removed: {id}");
                }
            }
        }

        public File Get(int id)
        {
            File result;
            using (var connection = new SqliteConnection(_connectionString))
            {
                result = _databaseInterface.ExecuteCustomQuery<File>(
                        "Select * From Files Where Files.Id = @id",
                        connection,
                        new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = id }
                    ).FirstOrDefault();
                if (result == null)
                {
                    _logger.LogError($"Element not find: {id}");
                }
            }
            return result;
        }

        public IEnumerable<File> GetAll()
        {
            IEnumerable<File> result;
            using (var connection = new SqliteConnection(_connectionString))
            {
                result = _databaseInterface.ExecuteCustomQuery<File>(
                        "Select * From Files Where Files.Id = @id",
                        connection
                    );
                if (result == null)
                {
                    _logger.LogError($"Elements not find");
                }
            }
            return result;
        }

        public void Update(File item)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                bool result = _databaseInterface.ExecuteCustomQuery(
                        "Update Files" +
                        "Set Name = @name, AsBytes = @asBytes, FileType = @fileType, Extension = @extention)" +
                        "Where Id = @id",
                        connection,
                        new SqliteParameter { ParameterName = "@name", DbType = DbType.String, Value = item.Name },
                        new SqliteParameter { ParameterName = "@asBytes", DbType = DbType.Binary, Value = item.AsBytes },
                        new SqliteParameter { ParameterName = "@fileType", DbType = DbType.String, Value = item.FileType },
                        new SqliteParameter { ParameterName = "@extention", DbType = DbType.String, Value = item.Extension },
                        new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = item.Id }
                    );
                if (!result)
                {
                    _logger.LogError($"Element not updated: {item.Id}, {item.Name}, {item.Extension}, {item.FileType}");
                }
            }
        }

        #endregion Public Methods
    }
}