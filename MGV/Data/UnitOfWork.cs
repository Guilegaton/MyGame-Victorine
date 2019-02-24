using MGV.Data.Repositories;
using MGV.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Data;

namespace MGV.Data
{
    public class UnitOfWork //: IUnitOfWork
    {
        #region Private Fields

        private readonly ILogger _logger;

        private IDbConnection _connection;
        private EndingRepository _endingRepository;
        private FileRepository _fileRepository;
        private QuestionRepository _questionRepository;
        private QuizRepository _quizRepository;
        private RuleRepository _ruleRepository;
        private StageRepository _stageRepository;
        private IDbTransaction _transaction;

        #endregion Private Fields

        #region Public Constructors

        public UnitOfWork(string connectionString, ILogger logger)
        {
            _logger = logger;

            _connection = new SqliteConnection(connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        #endregion Public Constructors

        #region Public Properties

        public EndingRepository EndingRepository => _endingRepository ?? (_endingRepository = new EndingRepository(_transaction, _logger));

        public FileRepository FileRepository => _fileRepository ?? (_fileRepository = new FileRepository(_transaction, _logger));

        public QuestionRepository QuestionRepository => _questionRepository ?? (_questionRepository = new QuestionRepository(_transaction, _logger));

        public QuizRepository QuizRepository => _quizRepository ?? (_quizRepository = new QuizRepository(_transaction, _logger));

        public RuleRepository RuleRepository => _ruleRepository ?? (_ruleRepository = new RuleRepository(_transaction, _logger));

        public StageRepository StageRepository => _stageRepository ?? (_stageRepository = new StageRepository(_transaction, _logger));

        #endregion Public Properties

        #region Public Methods

        public void Commit()
        {
            try
            {
                _transaction.Commit();
            }
            catch (Exception error)
            {
                _transaction.Rollback();
                _logger.LogError(error, error.Message);
            }
            finally
            {
                _transaction.Dispose();
                _transaction = _connection.BeginTransaction();
                ResetRepositories();
            }
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }

            if (_quizRepository != null)
            {
                _quizRepository.Dispose();
                _quizRepository = null;
            }

            if (_ruleRepository != null)
            {
                _ruleRepository.Dispose();
                _ruleRepository = null;
            }

            if (_stageRepository != null)
            {
                _stageRepository.Dispose();
                _stageRepository = null;
            }

            if (_questionRepository != null)
            {
                _questionRepository.Dispose();
                _questionRepository = null;
            }

            if (_endingRepository != null)
            {
                _endingRepository.Dispose();
                _endingRepository = null;
            }

            if (_fileRepository != null)
            {
                _fileRepository.Dispose();
                _fileRepository = null;
            }

            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }

        public void ResetRepositories()
        {
            _quizRepository = null;
            _ruleRepository = null;
            _stageRepository = null;
            _questionRepository = null;
            _endingRepository = null;
            _fileRepository = null;
        }

        #endregion Public Methods
    }
}