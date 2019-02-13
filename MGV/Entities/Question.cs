namespace MGV.Entities
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Questions")]
    public class Question : BaseEntity
    {
        #region Public Properties

        public string Category { get; set; }
        public int Cost { get; set; }
        public int StageId { get; set; }
        public QuestionTypes Type { get; set; }

        #endregion Public Properties

        #region Public Enums

        public enum QuestionTypes : long
        {
            Music,
            Image,
            Text,
            Video
        }

        #endregion Public Enums
    }
}