using System.Collections.Generic;

namespace MGV.Entities
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Quizzes")]
    public class Quiz : BaseEntity
    {
        #region Public Properties

        public IEnumerable<Rule> Rules { get; set; }
        public IEnumerable<Stage> Stages { get; set; }

        #endregion Public Properties
    }
}