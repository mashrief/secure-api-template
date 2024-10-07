using Microsoft.EntityFrameworkCore;

namespace RepositoryLayer.Data
{
    public class SecureDBContext : DbContext
    {
        public SecureDBContext(DbContextOptions<SecureDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Model Builder

            #endregion
        }

        #region DB Set



        #endregion
    }
}
