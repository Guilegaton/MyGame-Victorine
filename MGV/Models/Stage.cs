using System.Collections.Generic;

namespace MGV.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Stages")]
    public class Stage : BaseEntity
    {
        #region Public Properties

        public IEnumerable<Ending> Ending { get; set; }
        public IEnumerable<Question> Questions { get; set; }
        public int QuizId { get; set; }

        #endregion Public Properties
    }
}