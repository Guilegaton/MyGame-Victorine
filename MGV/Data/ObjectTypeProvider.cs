using MGV.Models;
using System;
using System.Collections.Generic;

namespace MGV.Data
{
    public static class ObjectTypeProvider
    {
        #region Private Fields

        private static readonly Dictionary<Type, ObjectType> Types = new Dictionary<Type, ObjectType>
        {
            {typeof(Quiz), ObjectType.Quiz },
            {typeof(Rule), ObjectType.Rule },
            {typeof(Stage), ObjectType.Stage },
            {typeof(Ending), ObjectType.Ending },
            {typeof(Question), ObjectType.Question },
        };

        #endregion Private Fields

        #region Public Enums

        public enum ObjectType
        {
            Quiz = 10,
            Rule = 20,
            Stage = 30,
            Ending = 40,
            Question = 50
        }

        #endregion Public Enums

        #region Public Methods

        public static ObjectType For(Type type)
        {
            return Types[type];
        }

        #endregion Public Methods
    }
}