using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace WebApiFile.DB.Entities
{
    public class Entity: IEntity
    {
        [Key]
        [XmlIgnore]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }

        [Column(Order = 1)]
        [XmlIgnore]
        [JsonIgnore]
        public DateTime Created { get; set; }

        [Column(Order = 2)]
        [XmlIgnore]
        [JsonIgnore]
        public DateTime Changed { get; set; }

        public virtual void BeforeInsert()
        {
            Created = DateTime.UtcNow;
            Changed = Created;
        }

        public virtual void BeforeUpdate()
        {
            Changed = DateTime.UtcNow;
        }
    }
}
