using MGV.Models;
using MGV.Shared;
using Microsoft.Extensions.Logging;

namespace MGV.Data.Repositories
{
    public class QuizzeRepository : GenericRepository<Quiz>, IRepository<Quiz>
    {
        #region Public Constructors

        public QuizzeRepository(string connectionString, ILogger logger) : base(connectionString, logger)
        {
        }

        #endregion Public Constructors
    }
}