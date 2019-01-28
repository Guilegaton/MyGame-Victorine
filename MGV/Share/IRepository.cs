using System.Collections.Generic;

namespace MGV.Share
{
    internal interface IRepository<T> where T : class
    {
        #region Public Methods

        void Create(T item);

        void Delete(int id);

        T Get(int id);

        IEnumerable<T> GetAll();

        void Update(T item);

        #endregion Public Methods
    }
}