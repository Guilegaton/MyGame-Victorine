using MGV.Entities;
using MGV.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MGV.Data.Repositories
{
    public class QuizRepository : GenericRepository<Quiz>, IRepository<Quiz>
    {
        #region Public Constructors

        public QuizRepository(SqliteConnection connectionString, ILogger logger) : base(connectionString, logger)
        {
        }

        #endregion Public Constructors
    }
}