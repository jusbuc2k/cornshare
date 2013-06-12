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
    [Table("SharedFileSet")]
    public class SharedFileSet
    {
        [Required]
        [MaxLength(64)]
        [Key]
        public byte[] ShareToken { get; set; }

        [Required]
        [StringLength(128)]
        public string OwnerUsername { get; set; }

        [Required]
        public int FileSetID { get; set; }

        [Required]
        [ForeignKey("FileSetID")]
        public virtual FileSet FileSet { get; set; }

        [MaxLength(64)]
        public byte[] Password { get; set; }

        public DateTime? ExpirationDateTime { get; set; }

        [Required]
        public bool AllowUpload { get; set; }

        [Required]
        public DateTime CreateDateTime { get; set; }

        [Required]
        [StringLength(128)]
        public string CreateUsername { get; set; }
    }
}
