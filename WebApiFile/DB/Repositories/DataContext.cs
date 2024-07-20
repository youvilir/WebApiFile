using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApiFile.DB.Entities;
using File = WebApiFile.DB.Entities.File;

namespace WebApiFile.DB.Repositories
{
    public class DataContext: DbContext
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

        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }

        internal void DetectChanges()
        {
            ChangeTracker.DetectChanges();
        }

    }

}


