using MGV.Models;
using MGV.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;

namespace MGV.Data.Repositories
{
    public class StageRepository : GenericRepository<Stage>, IRepository<Stage>
    {
        #region Public Constructors

        public StageRepository(SqliteConnection connectionString, ILogger logger) : base(connectionString, logger)
        {
        }

        #endregion Public Constructors

        #region Public Methods

        public IEnumerable<Stage> GetStagesByQuiz(int quizId)
        {
            IEnumerable<Stage> result;

            result = _databaseInterface.ExecuteCustomQuery<Stage>(
                    $"Select * From Stages Where Stages.QuizId = @id",
                    _connectionString,
                    new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = quizId });
            if (result == null)
            {
                _logger.LogError($"Stages not find with this quiz id: {quizId}");
                return result;
            }

            using (var fileObjRepo = new FileObjectRepository(_connectionString, _logger))
            {
                foreach (var item in result)
                {
                    item.Files = fileObjRepo.GetFiles(item);
                }
            }

            return result;
        }

        #endregion Public Methods
    }
}