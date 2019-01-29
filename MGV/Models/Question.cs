namespace MGV.Models
{
    public abstract class Question : BaseEntity
    {
        #region Public Properties

        public int Cost { get; set; }
        public int StageId { get; set; }

        #endregion Public Properties
    }
}