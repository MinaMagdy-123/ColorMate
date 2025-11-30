using ColorMate.EF.Repositories.Base;
using ColorMate.EF.Repositories.User;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Reflection.Metadata.BlobBuilder;

namespace ColorMate.EF.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IUserRepository Users { get; private set; }

        public UnitOfWork(ApplicationDbContext context, IUserRepository users)
        {
            _context = context;
            Users = users;
        }


        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

    }
}
