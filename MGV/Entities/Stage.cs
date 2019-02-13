using System.Collections.Generic;

namespace MGV.Entities
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Stages")]
    public class Stage : BaseEntity
    {
        #region Public Properties

        public IEnumerable<Ending> Endings { get; set; }
        public IEnumerable<Question> Questions { get; set; }
        public int QuizId { get; set; }
        public int StageNo { get; set; }

        #endregion Public Properties
    }
}