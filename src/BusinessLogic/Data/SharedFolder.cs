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
    [Table("SharedFolder")]
    public class SharedFolder
    {
        [Required]
        [StringLength(128)]
        public string OwnerUsername { get; set; }
        
        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [Required]
        [Key]
        [StringLength(128)]
        public string ShareToken { get; set; }

        public DateTime? ExpirationDateTime { get; set; }

        [Required]
        public DateTime CreateDateTime { get; set; }
        
        [Required]
        [StringLength(128)]
        public string CreateUsername { get; set; }
    }
}
