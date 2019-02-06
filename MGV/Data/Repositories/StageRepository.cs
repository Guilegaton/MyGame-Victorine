﻿using MGV.Models;
using MGV.Shared;
using Microsoft.Extensions.Logging;

namespace MGV.Data.Repositories
{
    public class StageRepository : GenericRepository<Stage>, IRepository<Stage>
    {
        #region Public Constructors

        public StageRepository(string connectionString, ILogger logger) : base(connectionString, logger)
        {
        }

        #endregion Public Constructors
    }
}