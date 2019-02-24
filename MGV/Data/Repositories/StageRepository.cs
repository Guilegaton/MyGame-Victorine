using Dapper;
using MGV.Entities;
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

        public StageRepository(IDbTransaction transaction, ILogger logger) : base(transaction, logger)
        {
        }

        #endregion Public Constructors

        #region Public Methods

        public IEnumerable<Stage> GetStagesByQuiz(int quizId)
        {
            var sql = $"Select * From Stages Where Stages.QuizId = @id";

            IEnumerable<Stage> result = _connectionString.Query<Stage>(sql, new { id = quizId });
            
            if (result == null)
            {
                _logger.LogError($"Stages not find with this quiz id: {quizId}");
                return result;
            }

            using (var fileObjRepo = new FileObjectRepository(_transaction, _logger))
            {
                foreach (var item in result)
                {
                    item.Files = fileObjRepo.GetAllFilesForObject(item);
                }
            }

            return result;
        }

        public void GetStageWithAllNestedEntities(ref Stage stage)
        {
            using (var endingRepo = new EndingRepository(_transaction, _logger))
            using (var questionRepo = new QuestionRepository(_transaction, _logger))
            {
                stage.Endings = endingRepo.GetEndingsByStage(stage.Id);
                stage.Questions = questionRepo.GetQuestionsByStage(stage.Id);
            }
        }

        #endregion Public Methods
    }
}