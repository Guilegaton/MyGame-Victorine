using MGV.Data.Repositories;
using MGV.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace MGV.Data
{
    public class UnitOfWork
    {
        #region Private Fields

        private readonly string _connectionString;
        private readonly ILogger _logger;

        #endregion Private Fields

        #region Public Constructors

        public UnitOfWork(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        #endregion Public Constructors

        #region Public Methods

        #region Create

        public void Create(Quiz item)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var repo = new QuizRepository(connection, _logger))
            {
                repo.Create(item);
            }
        }

        public void Create(Rule item)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var repo = new RuleRepository(connection, _logger))
            {
                repo.Create(item);
            }
        }

        public void Create(Stage item)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var repo = new StageRepository(connection, _logger))
            {
                repo.Create(item);
            }
        }

        public void Create(Ending item)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var repo = new EndingRepository(connection, _logger))
            {
                repo.Create(item);
            }
        }

        public void Create(Question item)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var repo = new QuestionRepository(connection, _logger))
            {
                repo.Create(item);
            }
        }

        #endregion Create

        #region Delete

        public void DeleteEnding(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var endingRepo = new EndingRepository(connection, _logger))
            {
                endingRepo.Delete(id);
            }
        }

        public void DeleteQuestion(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var questionRepo = new QuestionRepository(connection, _logger))
            {
                questionRepo.Delete(id);
            }
        }

        public void DeleteQuiz(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var repo = new QuizRepository(connection, _logger))
            {
                Quiz deletedObject = GetQuiz(id);

                using (var ruleRepo = new RuleRepository(connection, _logger))
                    foreach (var item in deletedObject.Rules)
                    {
                        ruleRepo.Delete(item.Id);
                    }

                using (var stageRepo = new StageRepository(connection, _logger))
                using (var endingRepo = new EndingRepository(connection, _logger))
                using (var questionRepo = new QuestionRepository(connection, _logger))
                {
                    foreach (var item in deletedObject.Stages)
                    {
                        DeleteAllStageNestedEntities(item, connection, endingRepo, questionRepo);
                        stageRepo.Delete(item.Id);
                    }
                }

                repo.Delete(id);
            }
        }

        public void DeleteRule(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var ruleRepo = new RuleRepository(connection, _logger))
            {
                ruleRepo.Delete(id);
            }
        }

        public void DeleteStage(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var stageRepo = new StageRepository(connection, _logger))
            using (var endingRepo = new EndingRepository(connection, _logger))
            using (var questionRepo = new QuestionRepository(connection, _logger))
            {
                Stage deletedObject = GetStage(id);
                DeleteAllStageNestedEntities(deletedObject, connection, endingRepo, questionRepo);
                stageRepo.Delete(id);
            }
        }

        #endregion Delete

        #region Get

        public Ending GetEnding(int id)
        {
            Ending result;

            using (var connection = new SqliteConnection(_connectionString))
            using (var repo = new EndingRepository(connection, _logger))
            {
                result = repo.Get(id);
            }
            return result;
        }

        public Question GetQuestion(int id)
        {
            Question result;

            using (var connection = new SqliteConnection(_connectionString))
            using (var repo = new QuestionRepository(connection, _logger))
            {
                result = repo.Get(id);
            }

            return result;
        }

        public Quiz GetQuiz(int id)
        {
            Quiz result;
            using (var connection = new SqliteConnection(_connectionString))
            {
                using (var repo = new QuizRepository(connection, _logger))
                {
                    result = repo.Get(id);
                }
                using (var repo = new StageRepository(connection, _logger))
                {
                    result.Stages = repo.GetStagesByQuiz(id);
                    using (var endingRepo = new EndingRepository(connection, _logger))
                    using (var questionRepo = new QuestionRepository(connection, _logger))
                    {
                        for (int i = 0; i < result.Stages.Count(); i++)
                        {
                            var stageList = result.Stages.ToList();
                            (stageList[i].Questions, stageList[i].Endings) = GetStageNestedEntities(stageList[i].Id, connection, endingRepo, questionRepo);
                        }
                    }
                }
                using (var repo = new RuleRepository(connection, _logger))
                {
                    result.Rules = repo.GetAll().Where(rule => rule.QuizId == id);
                }
            }
            return result;
        }

        public Rule GetRule(int id)
        {
            Rule result;

            using (var connection = new SqliteConnection(_connectionString))
            using (var repo = new RuleRepository(connection, _logger))
            {
                result = repo.Get(id);
            }

            return result;
        }

        public Stage GetStage(int id)
        {
            Stage result;
            using (var connection = new SqliteConnection(_connectionString))
            {
                using (var repo = new StageRepository(connection, _logger))
                {
                    result = repo.Get(id);
                }

                using (var endingRepo = new EndingRepository(connection, _logger))
                using (var questionRepo = new QuestionRepository(connection, _logger))
                {
                    (result.Questions, result.Endings) = GetStageNestedEntities(result.Id, connection, endingRepo, questionRepo);
                }
            }
            return result;
        }

        #endregion Get

        #region GetAll

        public IEnumerable<Ending> GetEndings()
        {
            IEnumerable<Ending> result = Enumerable.Empty<Ending>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                using (var repo = new EndingRepository(connection, _logger))
                {
                    result = repo.GetAll();
                }
            }
            return result;
        }


        public IEnumerable<Question> GetQuestions()
        {
            IEnumerable<Question> result = Enumerable.Empty<Question>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                using (var repo = new QuestionRepository(connection, _logger))
                {
                    result = repo.GetAll();
                }
            }
            return result;
        }


        public IEnumerable<Quiz> GetQuizzes()
        {
            List<Quiz> result = new List<Quiz>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                using (var repo = new QuizRepository(connection, _logger))
                {
                    result = repo.GetAll().ToList();
                }

                using (var stageRepo = new StageRepository(connection, _logger))
                using (var ruleRepository = new RuleRepository(connection, _logger))
                {
                    for (var i = 0; i < result.Count(); i++)
                    {
                        result[i].Rules = ruleRepository.GetRulesByQuiz(result[i].Id);
                        List<Stage> stages = stageRepo.GetStagesByQuiz(result[i].Id).ToList();

                        using (var endingRepo = new EndingRepository(connection, _logger))
                        using (var questionRepo = new QuestionRepository(connection, _logger))
                            for (int j = 0; j < stages.Count(); j++)
                            {
                                (stages[j].Questions, stages[j].Endings) = GetStageNestedEntities(stages[j].Id, connection, endingRepo, questionRepo);
                            }

                        result[i].Stages = stages;
                    }
                }
            }
            return result;
        }


        public IEnumerable<Rule> GetRules()
        {
            IEnumerable<Rule> result = Enumerable.Empty<Rule>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                using (var repo = new RuleRepository(connection, _logger))
                {
                    result = repo.GetAll();
                }
            }
            return result;
        }


        public IEnumerable<Stage> GetStages()
        {
            List<Stage> result = new List<Stage>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                using (var repo = new StageRepository(connection, _logger))
                {
                    result = repo.GetAll().ToList();
                }
                using (var endingRepo = new EndingRepository(connection, _logger))
                using (var questionRepo = new QuestionRepository(connection, _logger))
                    for (int i = 0; i < result.Count(); i++)
                    {
                        (result[i].Questions, result[i].Endings) = GetStageNestedEntities(result[i].Id, connection, endingRepo, questionRepo);
                    }
            }
            return result;
        }

        #endregion GetAll

        #region Update

        public void Update(Quiz item)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var repo = new QuizRepository(connection, _logger))
            {
                repo.Update(item);
                using (var ruleRepo = new RuleRepository(connection, _logger))
                {
                    foreach (var rule in item.Rules)
                    {
                        ruleRepo.Update(rule);
                    }
                }

                using (var stageRepo = new StageRepository(connection, _logger))
                using (var endingRepo = new EndingRepository(connection, _logger))
                using (var questionRepo = new QuestionRepository(connection, _logger))
                    foreach (var stage in item.Stages)
                    {
                        UpdateStageNestedEntities(stage, stageRepo, endingRepo, questionRepo);
                    }
            }
        }

        public void Update(Rule item)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var repo = new RuleRepository(connection, _logger))
            {
                repo.Update(item);
            }
        }

        public void Update(Stage item)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var stageRepo = new StageRepository(connection, _logger))
            using (var endingRepo = new EndingRepository(connection, _logger))
            using (var questionRepo = new QuestionRepository(connection, _logger))
            {
                UpdateStageNestedEntities(item, stageRepo, endingRepo, questionRepo);
            }
        }

        public void Update(Question item)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var repo = new QuestionRepository(connection, _logger))
            {
                repo.Update(item);
            }
        }

        public void Update(Ending item)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var repo = new EndingRepository(connection, _logger))
            {
                repo.Update(item);
            }
        }

        #endregion Update

        #endregion Public Methods

        #region Private Methods

        private void DeleteAllStageNestedEntities(Stage stage, SqliteConnection connection, EndingRepository endingRepo, QuestionRepository questionRepo)
        {
            foreach (var item in stage.Endings)
            {
                endingRepo.Delete(item.Id);
            }
            foreach (var item in stage.Questions)
            {
                questionRepo.Delete(item.Id);
            }
        }

        private (IEnumerable<Question> Questions, IEnumerable<Ending> Endings) GetStageNestedEntities(int stageId, SqliteConnection connection, EndingRepository endingRepo, QuestionRepository questionRepo)
        {
            return (questionRepo.GetAll().Where(question => question.StageId == stageId),
                    endingRepo.GetAll().Where(ending => ending.StageId == stageId));
        }

        private void UpdateStageNestedEntities(Stage stage, StageRepository stageRepo, EndingRepository endingRepo, QuestionRepository questionRepo)
        {
            foreach (var item in stage.Endings)
            {
                endingRepo.Update(item);
            }
            foreach (var item in stage.Questions)
            {
                questionRepo.Update(item);
            }
            stageRepo.Update(stage);
        }

        #endregion Private Methods
    }
}