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
    [Table("FileSet")]
    public class FileSet
    {
        [Required]
        public int FileSetID { get; set; }

        [Required]
        public string Name { get; set; }

        [StringLength(128)]
        [Required]
        public string OwnerUsername { get; set; }

        [Required]
        public DateTime CreateDateTime { get; set; }

        public DateTime? ExpirationDateTime { get; set; }

        public bool IsDraft { get; set; }

        public virtual ICollection<StoredFile> StoredFiles { get; set; }

        public virtual ICollection<SharedFileSet> SharedFileSets { get; set; }
    }
}