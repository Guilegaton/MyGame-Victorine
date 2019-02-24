using Dapper;
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

        public EndingRepository(IDbTransaction transaction, ILogger logger) : base(transaction, logger)
        {
        }

        #endregion Public Constructors

        #region Public Methods

        public IEnumerable<Ending> GetEndingsByStage(int stageId)
        {
            var sql = $"Select * From Endings Where Endings.StageId = @id";

            IEnumerable<Ending> result = _connectionString.Query<Ending>(sql, new { id = stageId });
            
            if (result == null)
            {
                _logger.LogError($"Endings not find with this stage id: {stageId}");
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