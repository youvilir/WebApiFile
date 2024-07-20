using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WebApiFile.DB.Repositories.EntityRepository;

namespace WebApiFile.DB.Repositories
{
    public class Repository
    {
        protected DataContext _dataContext { get; set; }

        public Repository(DataContext dataContext)
        {
            _dataContext = dataContext;

            Files = new FileRepository(dataContext);
            Codes = new CodeForDeleteRepository(dataContext);
        }

        public FileRepository Files { get; }

        public CodeForDeleteRepository Codes { get; }

        public void SaveChanges()
        {
            _dataContext.SaveChanges();
        }

        public async Task SaveChangesAsync()
        {
            await _dataContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _dataContext?.Dispose();
        }

    }
}
