using MGV.Models;
using MGV.Share;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MGV.Data.Repositories
{
    public class EndingRepository : IRepository<Ending>
    {
        #region Private Fields

        private readonly string _connectionString;
        private readonly DatabaseInterface _databaseInterface;
        private readonly ILogger _logger;
        private bool _isDisposed = false;

        #endregion Private Fields

        #region Public Constructors

        public EndingRepository(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
            _databaseInterface = DatabaseInterface.GetInstance(_logger);
        }

        #endregion Public Constructors

        #region Public Methods

        public void Create(Ending item)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                bool result = _databaseInterface.ExecuteCustomQuery(
                        "INSERT INTO Endings(Name, Description, CreatedAt, StageId)" +
                        "Values(@name, @description, @createdAt, @stageId)",
                        connection,
                        new SqliteParameter { ParameterName = "@name", DbType = DbType.String, Value = item.Name },
                        new SqliteParameter { ParameterName = "@description", DbType = DbType.Binary, Value = item.Description },
                        new SqliteParameter { ParameterName = "@createdAt", DbType = DbType.String, Value = DateTime.Now.ToString() },
                        new SqliteParameter { ParameterName = "@stageId", DbType = DbType.String, Value = item.StageId }
                    );
                if (!result)
                {
                    _logger.LogError($"Ending not created: {item.Name}");
                }
            }

            using (var fileObjRepository = new FileObjectRepository(_connectionString, _logger))
            {
                foreach (var file in item.Files)
                {
                    fileObjRepository.AddFileToObject(item, file.Id);
                }
            }
        }

        public void Delete(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                bool result = _databaseInterface.ExecuteCustomQuery(
                        "Delete From Endings Where Endings.Id = @id",
                        connection,
                        new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = id }
                    );
                if (!result)
                {
                    _logger.LogError($"Stage not removed: {id}");
                }
            }
            using (var fileObjRepository = new FileObjectRepository(_connectionString, _logger))
            {
                fileObjRepository.DeleteAllFilesFromObject<Ending>(id);
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        public Ending Get(int id)
        {
            Ending result;
            using (var connection = new SqliteConnection(_connectionString))
            {
                result = _databaseInterface.ExecuteCustomQuery<Ending>(
                        "Select * From Endings Where Endings.Id = @id",
                        connection,
                        new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = id }
                    ).FirstOrDefault();
                if (result == null)
                {
                    _logger.LogError($"Ending not find: {id}");
                    return result;
                }
            }

            using (var fileObjRepo = new FileObjectRepository(_connectionString, _logger))
            {
                result.Files = fileObjRepo.GetFiles(result);
            }

            return result;
        }

        public IEnumerable<Ending> GetAll()
        {
            IEnumerable<Ending> result = Enumerable.Empty<Ending>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                result = _databaseInterface.ExecuteCustomQuery<Ending>(
                        "Select * From Endings",
                        connection
                    );
                if (result == null)
                {
                    _logger.LogError($"Endings not found");
                }
            }

            using (var fileObjRepo = new FileObjectRepository(_connectionString, _logger))
            {
                foreach (var element in result)
                {
                    element.Files = fileObjRepo.GetFiles(element);
                }
            }

            return result;
        }

        public void Update(Ending item)
        {
            Ending oldItem = Get(item.Id);

            using (var connection = new SqliteConnection(_connectionString))
            {
                bool result = _databaseInterface.ExecuteCustomQuery(
                        "Update Endings" +
                        "Set Name = @name, Description = @description)" +
                        "Where Id = @id",
                        connection,
                        new SqliteParameter { ParameterName = "@name", DbType = DbType.String, Value = item.Name },
                        new SqliteParameter { ParameterName = "@description", DbType = DbType.String, Value = item.Description },
                        new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = item.Id }
                    );
                if (!result)
                {
                    _logger.LogError($"Ending not updated: {item.Id}, {item.Name}");
                }
            }

            using (var fileObjRepo = new FileObjectRepository(_connectionString, _logger))
            {
                fileObjRepo.UpdateFilesForObject(item, oldItem);
            }
        }

        #endregion Public Methods
    }
}