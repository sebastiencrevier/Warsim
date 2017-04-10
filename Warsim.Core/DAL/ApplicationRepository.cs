using System;

namespace Warsim.Core.DAL
{
    public abstract class ApplicationRepository : IDisposable
    {
        protected ApplicationDbContext DbContext { get; }

        protected ApplicationRepository(ApplicationDbContext dbContext)
        {
            this.DbContext = dbContext;
        }

        public void Dispose()
        {
            this.DbContext.Dispose();
        }
    }
}