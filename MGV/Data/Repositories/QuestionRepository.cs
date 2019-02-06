using MGV.Models;
using MGV.Shared;
using Microsoft.Extensions.Logging;

namespace MGV.Data.Repositories
{
    public class QuestionRepository : GenericRepository<Question>, IRepository<Question>
    {
        #region Public Constructors

        public QuestionRepository(string connectionString, ILogger logger) : base(connectionString, logger)
        {
        }

        #endregion Public Constructors
    }
}