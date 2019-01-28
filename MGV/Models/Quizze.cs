using System.Collections.Generic;

namespace MGV.Models
{
    public class Quizze : BaseEntity
    {
        #region Public Properties

        public IEnumerable<Rule> Rules { get; set; }
        public IEnumerable<Stage> Stages { get; set; }

        #endregion Public Properties
    }
}