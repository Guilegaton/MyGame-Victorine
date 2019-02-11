﻿namespace MGV.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Questions")]
    public class Question : BaseEntity
    {
        #region Public Properties

        public int Cost { get; set; }
        public QuestionTypes Type { get; set; }
        public int StageId { get; set; }

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