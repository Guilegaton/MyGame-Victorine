namespace MGV.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Endings")]
    public class Ending : BaseEntity
    {
        #region Public Properties

        public int StageId { get; set; }
        public bool IsSelected { get; set; }

        #endregion Public Properties
    }
}