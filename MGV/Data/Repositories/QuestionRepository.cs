using Dapper;
using MGV.Entities;
using MGV.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;

namespace MGV.Data.Repositories
{
    public class QuestionRepository : GenericRepository<Question>, IRepository<Question>
    {
        #region Public Constructors

        public QuestionRepository(IDbTransaction transaction, ILogger logger) : base(transaction, logger)
        {
        }

        #endregion Public Constructors

        #region Public Methods

        public IEnumerable<Question> GetQuestionsByStage(int stageId)
        {
            var sql = $"Select * From Questions Where Questions.StageId = @id";

            IEnumerable<Question> result = _connectionString.Query<Question>(sql, new { id = stageId });

            if (result == null)
            {
                _logger.LogError($"Questions not find with this stage id: {stageId}");
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

        #endregion Public Methods
    }
}