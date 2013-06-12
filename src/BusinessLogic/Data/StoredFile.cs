using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessLogic.Data
{
    [Table("StoredFile")]
    public class StoredFile
    {
        [StringLength(128)]
        [Required]
        public string OwnerUsername { get; set; }

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int FileID { get; set; }

        [Required]
        [StringLength(255)]
        public string Filename { get; set; }

        [Required]
        public int FileSetID { get; set; }

        [Required]
        [ForeignKey("FileSetID")]
        public virtual FileSet FileSet { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [StringLength(255)]
        public string StoragePath { get; set; }

        [Required]
        public long Length { get; set; }

        [Required]
        [StringLength(255)]
        public string ContentType { get; set; }

        [Required]
        public DateTime CreateDateTime { get; set; }

        [Required]
        [StringLength(128)]
        public string CreateUsername { get; set; }

        public virtual ICollection<SharedFile> SharedFiles { get; set; }
    }
}
