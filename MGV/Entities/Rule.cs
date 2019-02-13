namespace MGV.Entities
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Rules")]
    public class Rule : BaseEntity
    {
        #region Public Properties

        public int QuizId { get; set; }

        #endregion Public Properties
    }
}