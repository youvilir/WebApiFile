using Newtonsoft.Json;

namespace WebApiFile.DB.Entities
{
    public class File : Entity
    {
        public string Name { get; set; }

        public string Extension { get; set; }

        public string ContentType { get; set; }

        public string ContentDescription { get; set; }

        public long Size { get; set; }

        public byte[] Content { get; set; }

    }
}
