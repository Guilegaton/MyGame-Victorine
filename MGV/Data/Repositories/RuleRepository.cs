using Dapper;
using MGV.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using Rule = MGV.Entities.Rule;

namespace MGV.Data.Repositories
{
    public class RuleRepository : GenericRepository<Rule>, IRepository<Rule>
    {
        #region Public Constructors

        public RuleRepository(IDbTransaction transaction, ILogger logger) : base(transaction, logger)
        {
        }

        #endregion Public Constructors

        #region Public Methods

        public IEnumerable<Rule> GetRulesByQuiz(int quizId)
        {
            var sql = $"Select * From Rules Where Rules.QuizId = @id";

            IEnumerable<Rule> result = _connectionString.Query<Rule>(sql, new { id = quizId });

            if (result == null)
            {
                _logger.LogError($"Rules not find with this quiz id: {quizId}");
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