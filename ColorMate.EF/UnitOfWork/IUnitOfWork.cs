using ColorMate.EF.Repositories.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorMate.EF.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        int Complete();
    }
}
