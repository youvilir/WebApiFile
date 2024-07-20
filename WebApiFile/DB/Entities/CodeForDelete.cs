namespace WebApiFile.DB.Entities
{
	public class CodeForDelete: Entity
	{
		public Guid FileId { get; set; }

		public string Code { get; set; }

		public DateTime TimeTo { get; set; }
	}
}
