namespace WebApiFile.DB.Entities
{
    public interface IEntity
    {
        Guid? ID { get; }

        DateTime? Created { get; }

        DateTime? Changed { get; }

        void BeforeInsert();

        void BeforeUpdate();

    }
}
