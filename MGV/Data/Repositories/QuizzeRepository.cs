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
    public class QuizzeRepository : IRepository<Quiz>
    {
        #region Private Fields

        private readonly string _connectionString;
        private readonly DatabaseInterface _databaseInterface;
        private readonly ILogger _logger;
        private bool _isDisposed = false;

        #endregion Private Fields

        #region Public Constructors

        public QuizzeRepository(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
            _databaseInterface = DatabaseInterface.GetInstance(_logger);
        }

        #endregion Public Constructors

        #region Public Methods

        public void Create(Quiz item)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                bool result = _databaseInterface.ExecuteCustomQuery(
                        "INSERT INTO Quizzes(Name, Description, CreatedAt)" +
                        "Values(@name, @description, @createdAt)",
                        connection,
                        new SqliteParameter { ParameterName = "@name", DbType = DbType.String, Value = item.Name },
                        new SqliteParameter { ParameterName = "@description", DbType = DbType.Binary, Value = item.Description },
                        new SqliteParameter { ParameterName = "@createdAt", DbType = DbType.String, Value = DateTime.Now.ToString() }
                    );
                if (!result)
                {
                    _logger.LogError($"Quiz not created: {item.Name}");
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
                        "Delete From Quizzes Where Quizzes.Id = @id",
                        connection,
                        new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = id }
                    );
                if (!result)
                {
                    _logger.LogError($"Quiz not removed: {id}");
                }
            }
            using (var fileObjRepository = new FileObjectRepository(_connectionString, _logger))
            {
                fileObjRepository.DeleteAllFilesFromObject<Quiz>(id);
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

        public Quiz Get(int id)
        {
            Quiz result;
            using (var connection = new SqliteConnection(_connectionString))
            {
                result = _databaseInterface.ExecuteCustomQuery<Quiz>(
                        "Select * From Quizzes Where Quizzes.Id = @id",
                        connection,
                        new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = id }
                    ).FirstOrDefault();
                if (result == null)
                {
                    _logger.LogError($"Quiz not find: {id}");
                    return result;
                }
            }

            result.Files = GetFiles(result);

            return result;
        }

        public IEnumerable<Quiz> GetAll()
        {
            IEnumerable<Quiz> result = Enumerable.Empty<Quiz>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                result = _databaseInterface.ExecuteCustomQuery<Quiz>(
                        "Select * From Quizzes",
                        connection
                    );
                if (result == null)
                {
                    _logger.LogError($"Quizzes not find");
                }
            }

            foreach (var quiz in result)
            {
                quiz.Files = GetFiles(quiz);
            }

            return result;
        }

        public void Update(Quiz item)
        {
            Quiz oldItem = Get(item.Id);

            using (var connection = new SqliteConnection(_connectionString))
            {
                bool result = _databaseInterface.ExecuteCustomQuery(
                        "Update Quizzes" +
                        "Set Name = @name, Description = @description)" +
                        "Where Id = @id",
                        connection,
                        new SqliteParameter { ParameterName = "@name", DbType = DbType.String, Value = item.Name },
                        new SqliteParameter { ParameterName = "@description", DbType = DbType.String, Value = item.Description },
                        new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = item.Id }
                    );
                if (!result)
                {
                    _logger.LogError($"File not updated: {item.Id}, {item.Name}");
                }
            }

            UpdateFilesForQuiz(item, oldItem);
        }

        #endregion Public Methods

        #region Private Methods

        private IEnumerable<File> GetFiles(Quiz item)
        {
            IEnumerable<int> filesIds;
            IEnumerable<File> result;

            using (var fileObjRepository = new FileObjectRepository(_connectionString, _logger))
            {
                filesIds = fileObjRepository.GetAllFileIdForObject(item);
            }
            using (var fileRepo = new FileRepository(_connectionString, _logger))
            {
                result = filesIds.Select(fileId => fileRepo.Get(fileId));
            }

            return result;
        }

        private void UpdateFilesForQuiz(Quiz newItem, Quiz oldItem)
        {
            using (var fileObjRepo = new FileObjectRepository(_connectionString, _logger))
            {
                foreach (var newFile in newItem.Files.Except(oldItem.Files))
                {
                    fileObjRepo.AddFileToObject(newItem, newFile.Id);
                }
                foreach (var oldFile in oldItem.Files.Except(newItem.Files))
                {
                    fileObjRepo.DeleteFileFromObject(newItem, oldFile.Id);
                }
            }
        }

        #endregion Private Methods
    }
}