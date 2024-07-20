namespace WebApiFile.DB.Entities
{
    public interface IEntity
    {
        Guid ID { get; }

        DateTime Created { get; set; }

        DateTime Changed { get; set; }

        void BeforeInsert();

        void BeforeUpdate();

    }
}
