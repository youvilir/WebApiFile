using WebApiFile.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebApiFile.DB.Repositories
{
    public class RepositoryBase<T> where T: class, IEntity
    {
        protected DataContext _dataContext { get; set; }

        protected DbSet<T> Set { get; set; }

        public RepositoryBase(DataContext dataContext)
        {
            _dataContext = dataContext;
            Set = _dataContext.Set<T>();
        }

        public void Add(T entity)
        {
            entity.BeforeInsert();
            Set.Add(entity);
        }

        public async Task AddAsync(T entity)
        {
            entity.BeforeInsert();
            await Set.AddAsync(entity);
        }

        public void Update(T entity)
        {
            entity.BeforeUpdate();
            Set.Attach(entity);
            _dataContext.Entry(entity).State = EntityState.Modified;
            _dataContext.DetectChanges();
        }

        public virtual void Delete(Guid id)
        {
            var entity = Get(id);

            if (entity == null)
            {
                throw new Exception("Такого ID не существует");
            }

            Set.Remove(entity);
            _dataContext.DetectChanges();
        }

		public virtual void Delete(T entity)
        {
            Set.Remove(entity);
            _dataContext.DetectChanges();
        }

        public virtual T? Get(Guid id)
        {
            var value = Set.SingleOrDefault(e => e.ID == id);

            //if (value == null)
            //{
            //    throw new Exception("Такого ID не существует");
            //}

            return value;
        }

        public virtual IEnumerable<T> GetAll()
        {
            return Set.ToList();
        }

        public virtual T GetLast()
        {
            var value = Set.OrderBy(e => e.ID).Last();

            if (value == null)
            {
                throw new Exception("Такого ID не существует");
            }

            return value;
        }

        internal void DetectChanges()
        {
            _dataContext.DetectChanges();
        }

    }
}
