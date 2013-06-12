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
    [Table("SharedFile")]
    public class SharedFile
    {
        [Required]
        [StringLength(128)]
        public string OwnerUsername { get; set; }

        [Required]
        [MaxLength(32)]
        [Column(Order = 0)]
        [Key]
        public byte[] ShareToken { get; set; }

        [Required]
        [Column(Order = 1)]
        public int FileID { get; set; }

        [Required]
        [ForeignKey("FileID")]
        public StoredFile StoredFile { get; set; }

        [MaxLength(64)]
        public byte[] Password { get; set; }

        public DateTime? ExpirationDateTime { get; set; }

        [Required]
        public DateTime CreateDateTime { get; set; }

        [Required]
        [StringLength(128)]
        public string CreateUsername { get; set; }

    }

}
