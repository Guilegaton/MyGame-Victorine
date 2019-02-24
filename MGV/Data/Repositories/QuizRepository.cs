using MGV.Entities;
using MGV.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Data;

namespace MGV.Data.Repositories
{
    public class QuizRepository : GenericRepository<Quiz>, IRepository<Quiz>
    {
        #region Public Constructors

        public QuizRepository(IDbTransaction transaction, ILogger logger) : base(transaction, logger)
        {
        }

        #endregion Public Constructors

        public void GetQuizWithAllNestedEntities(ref Quiz quiz)
        {
            using (var ruleRepo = new RuleRepository(_transaction, _logger))
                quiz.Rules = ruleRepo.GetRulesByQuiz(quiz.Id);

            using (var questionRepo = new QuestionRepository(_transaction, _logger))
            using (var endingRepo = new EndingRepository(_transaction, _logger))
            using (var stageRepo = new StageRepository(_transaction, _logger))
            {
                var stages = stageRepo.GetStagesByQuiz(quiz.Id);
                foreach (var stage in stages)
                {
                    stage.Endings = endingRepo.GetEndingsByStage(stage.Id);
                    stage.Questions = questionRepo.GetQuestionsByStage(stage.Id);
                }
                quiz.Stages = stages;
            }
        }
    }
}