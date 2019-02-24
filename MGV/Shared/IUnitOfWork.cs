using System;

namespace MGV.Shared
{
    public interface IUnitOfWork : IDisposable
    {
        #region Public Methods

        void Commit();

        void ResetRepositories();

        #endregion Public Methods
    }
}