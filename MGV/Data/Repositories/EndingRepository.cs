using MGV.Entities;
using MGV.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;

namespace MGV.Data.Repositories
{
    public class EndingRepository : GenericRepository<Ending>, IRepository<Ending>
    {
        #region Public Constructors

        public EndingRepository(SqliteConnection connectionString, ILogger logger) : base(connectionString, logger)
        {
        }

        #endregion Public Constructors

        #region Public Methods

        public IEnumerable<Ending> GetEndingsByStage(int stageId)
        {
            IEnumerable<Ending> result;

            result = _databaseInterface.ExecuteCustomQuery<Ending>(
                    $"Select * From Endings Where Endings.StageId = @id",
                    _connectionString,
                    new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = stageId });
            if (result == null)
            {
                _logger.LogError($"Endings not find with this stage id: {stageId}");
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