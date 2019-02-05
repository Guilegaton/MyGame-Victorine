namespace MGV.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Questions")]
    public class Question : BaseEntity
    {
        #region Public Properties

        public int Cost { get; set; }
        public int StageId { get; set; }

        #endregion Public Properties
    }
}