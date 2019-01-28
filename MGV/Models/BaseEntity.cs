using System;

namespace MGV.Models
{
    public class BaseEntity
    {
        #region Public Properties

        public DateTime CreatedAt { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }

        #endregion Public Properties
    }
}