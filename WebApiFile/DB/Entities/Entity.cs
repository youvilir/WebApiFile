using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
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
        public Guid? ID { get; set; }

        [DisplayName("Создано")]
        [Column(Order = 1)]
        [Required]
        [XmlIgnore]
        [JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual DateTime? Created { get; set; }

        [DisplayName("Дата обновления")]
        [Column(Order = 2)]
        [XmlIgnore]
        [JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual DateTime? Changed { get; set; }

        public virtual void BeforeInsert()
        {
            if (ID == null) ID = Guid.NewGuid();
            Created = DateTime.UtcNow;
            Changed = Created;
        }

        public virtual void BeforeUpdate()
        {
            Changed = DateTime.UtcNow;
        }
    }
}
