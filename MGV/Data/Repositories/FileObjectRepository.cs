using MGV.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MGV.Data.Repositories
{
    public class FileObjectRepository
    {
        #region Private Fields

        private readonly string _connectionString;
        private readonly DatabaseInterface _databaseInterface;
        private readonly ILogger _logger;

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

        public void DeleteFileFromObject(BaseEntity item, int fileId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                bool result = _databaseInterface.ExecuteCustomQuery(
                        "Delete From Files_Objects " +
                        "Where FileID = @fileId and ObjectId = @objectId and ObjectType = @objectType",
                        connection,
                        new SqliteParameter { ParameterName = "@fileId", DbType = DbType.Int32, Value = fileId },
                        new SqliteParameter { ParameterName = "@ObjectId", DbType = DbType.Int32, Value = item.Id },
                        new SqliteParameter { ParameterName = "@fileId", DbType = DbType.Int32, Value = ObjectTypeProvider.For(item.GetType()) }
                    );
                if (!result)
                {
                    _logger.LogError($"FileObject not removed: {fileId}, {item.Id}, {item.GetType()}");
                }
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