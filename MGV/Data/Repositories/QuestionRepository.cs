using MGV.Models;
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

        public QuestionRepository(SqliteConnection connectionString, ILogger logger) : base(connectionString, logger)
        {
        }

        #endregion Public Constructors

        #region Public Methods

        public IEnumerable<Question> GetQuestionsByStage(int stageId)
        {
            IEnumerable<Question> result;

            result = _databaseInterface.ExecuteCustomQuery<Question>(
                    $"Select * From Questions Where Questions.StageId = @id",
                    _connectionString,
                    new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = stageId });
            if (result == null)
            {
                _logger.LogError($"Questions not find with this stage id: {stageId}");
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