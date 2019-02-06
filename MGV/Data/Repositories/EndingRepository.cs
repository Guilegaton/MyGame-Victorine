using MGV.Models;
using MGV.Shared;
using Microsoft.Extensions.Logging;

namespace MGV.Data.Repositories
{
    public class EndingRepository : GenericRepository<Ending>, IRepository<Ending>
    {
        #region Public Constructors

        public EndingRepository(string connectionString, ILogger logger) : base(connectionString, logger)
        {
        }

        #endregion Public Constructors
    }
}