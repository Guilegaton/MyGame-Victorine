using System;
using System.Collections.Generic;

namespace MGV.Shared
{
    internal interface IRepository<T> : IDisposable where T : new()
    {
        #region Public Methods

        void Create(T item);

        void Delete(int id);

        T Get(int id);

        T Get(string name);

        IEnumerable<T> GetAll();

        void Update(T item);

        #endregion Public Methods
    }
}