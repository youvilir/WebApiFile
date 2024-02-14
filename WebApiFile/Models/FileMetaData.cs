namespace WebApiFile.Models
{
    public class FileMetaData
    {
        public Guid? ID { get; set; }

        public string Name { get; set; }

        public string ContentType { get; set; }

        public string ContentDescription { get; set; }

        public long Size { get; set; }
    }
}
