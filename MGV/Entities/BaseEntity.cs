using System;
using System.Collections.Generic;

namespace MGV.Entities
{
    public class BaseEntity
    {
        #region Public Properties

        public DateTime CreatedAt { get; set; }
        public string Description { get; set; }
        public IEnumerable<File> Files { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }

        #endregion Public Properties
    }
}