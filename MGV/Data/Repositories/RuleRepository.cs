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

        public RuleRepository(SqliteConnection connectionString, ILogger logger) : base(connectionString, logger)
        {
        }

        #endregion Public Constructors

        #region Public Methods

        public IEnumerable<Rule> GetRulesByQuiz(int quizId)
        {
            IEnumerable<Rule> result;

            result = _databaseInterface.ExecuteCustomQuery<Rule>(
                    $"Select * From Rules Where Rules.QuizId = @id",
                    _connectionString,
                    new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = quizId });
            if (result == null)
            {
                _logger.LogError($"Rules not find with this quiz id: {quizId}");
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