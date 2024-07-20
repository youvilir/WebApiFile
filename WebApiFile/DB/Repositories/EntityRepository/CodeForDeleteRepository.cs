using Microsoft.EntityFrameworkCore;
using WebApiFile.DB.Entities;

namespace WebApiFile.DB.Repositories.EntityRepository
{
	public class CodeForDeleteRepository: RepositoryBase<CodeForDelete>
	{
		public CodeForDeleteRepository(DataContext dataContext) : base(dataContext)
		{

		}

		public Task<List<CodeForDelete>> GetCodesById(Guid id)
		{
			return Set.Where(x => x.FileId == id).ToListAsync();
		}
	}
}
