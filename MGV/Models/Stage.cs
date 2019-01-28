using System.Collections.Generic;

namespace MGV.Models
{
    public class Stage : BaseEntity
    {
        #region Public Properties

        public Ending Ending { get; set; }
        public IEnumerable<Question> Questions { get; set; }

        #endregion Public Properties
    }
}