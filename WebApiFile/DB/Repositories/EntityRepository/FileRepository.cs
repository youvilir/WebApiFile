using File = WebApiFile.DB.Entities.File;

namespace WebApiFile.DB.Repositories.EntityRepository
{
    public class FileRepository : RepositoryBase<File>
    {
        public FileRepository(DataContext dataContext) : base(dataContext)
        {
        }
    }
}
