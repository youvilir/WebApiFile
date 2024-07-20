using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApiFile.DB.Entities;
using File = WebApiFile.DB.Entities.File;

namespace WebApiFile.DB
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            Database.EnsureCreated(); // создание бд
        }

        public DbSet<File> Files { get; set; }

        public DbSet<CodeForDelete> CodesForDelete { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(message => Debug.WriteLine(message));
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            if (!ChangeTracker.HasChanges())
                return Task.FromResult(0);

            var changedEntities = ChangeTracker.Entries();

            foreach (var changedEntity in changedEntities)
            {
                if (changedEntity.Entity is not IEntity entity)
                    continue;

                switch (changedEntity.State)
                {
                    case EntityState.Added:
                        entity.BeforeInsert();
                        break;

                    case EntityState.Modified:
                        entity.BeforeUpdate();
                        break;

                    case EntityState.Detached:
                        break;

                    case EntityState.Unchanged:
                        break;

                    case EntityState.Deleted:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        internal void DetectChanges()
        {
            ChangeTracker.DetectChanges();
        }

    }

}


