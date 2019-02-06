using MGV.Shared;
using Microsoft.Extensions.Logging;
using Rule = MGV.Models.Rule;

namespace MGV.Data.Repositories
{
    public class RuleRepository : GenericRepository<Rule>, IRepository<Rule>
    {
        #region Public Constructors

        public RuleRepository(string connectionString, ILogger logger) : base(connectionString, logger)
        {
        }

        #endregion Public Constructors
    }
}