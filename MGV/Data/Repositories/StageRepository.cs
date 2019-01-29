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
    public class StageRepository : IRepository<Stage>
    {
        #region Private Fields

        private readonly string _connectionString;
        private readonly DatabaseInterface _databaseInterface;
        private readonly ILogger _logger;
        private bool _isDisposed = false;

        #endregion Private Fields

        #region Public Constructors

        public StageRepository(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
            _databaseInterface = DatabaseInterface.GetInstance(_logger);
        }

        #endregion Public Constructors

        #region Public Methods

        public void Create(Stage item)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                bool result = _databaseInterface.ExecuteCustomQuery(
                        "INSERT INTO Stages(Name, Description, CreatedAt, QuizId)" +
                        "Values(@name, @description, @createdAt, @quizId)",
                        connection,
                        new SqliteParameter { ParameterName = "@name", DbType = DbType.String, Value = item.Name },
                        new SqliteParameter { ParameterName = "@description", DbType = DbType.Binary, Value = item.Description },
                        new SqliteParameter { ParameterName = "@createdAt", DbType = DbType.String, Value = DateTime.Now.ToString() },
                        new SqliteParameter { ParameterName = "@quizId", DbType = DbType.String, Value = item.QuizId }
                    );
                if (!result)
                {
                    _logger.LogError($"Stage not created: {item.Name}");
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
                        "Delete From Stages Where Rules.Id = @id",
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
                fileObjRepository.DeleteAllFilesFromObject<Stage>(id);
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

        public Stage Get(int id)
        {
            Stage result;
            using (var connection = new SqliteConnection(_connectionString))
            {
                result = _databaseInterface.ExecuteCustomQuery<Stage>(
                        "Select * From Stages Where Stages.Id = @id",
                        connection,
                        new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = id }
                    ).FirstOrDefault();
                if (result == null)
                {
                    _logger.LogError($"Stage not find: {id}");
                    return result;
                }
            }

            using (var fileObjRepo = new FileObjectRepository(_connectionString, _logger))
            {
                result.Files = fileObjRepo.GetFiles(result);
            }

            return result;
        }

        public IEnumerable<Stage> GetAll()
        {
            IEnumerable<Stage> result = Enumerable.Empty<Stage>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                result = _databaseInterface.ExecuteCustomQuery<Stage>(
                        "Select * From Stages",
                        connection
                    );
                if (result == null)
                {
                    _logger.LogError($"Stages not found");
                }
            }

            using (var fileObjRepo = new FileObjectRepository(_connectionString, _logger))
            {
                foreach (var quiz in result)
                {
                    quiz.Files = fileObjRepo.GetFiles(quiz);
                }
            }

            return result;
        }

        public void Update(Stage item)
        {
            Stage oldItem = Get(item.Id);

            using (var connection = new SqliteConnection(_connectionString))
            {
                bool result = _databaseInterface.ExecuteCustomQuery(
                        "Update Stages" +
                        "Set Name = @name, Description = @description)" +
                        "Where Id = @id",
                        connection,
                        new SqliteParameter { ParameterName = "@name", DbType = DbType.String, Value = item.Name },
                        new SqliteParameter { ParameterName = "@description", DbType = DbType.String, Value = item.Description },
                        new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = item.Id }
                    );
                if (!result)
                {
                    _logger.LogError($"Stage not updated: {item.Id}, {item.Name}");
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