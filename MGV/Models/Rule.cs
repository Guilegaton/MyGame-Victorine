namespace MGV.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Rules")]
    public class Rule : BaseEntity
    {
        #region Public Properties

        public int QuizId { get; set; }

        #endregion Public Properties
    }
}