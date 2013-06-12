using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessLogic.Data
{
    public class SharingDataContext : DbContext
    {
        public SharingDataContext()
            : base("CornShare")
        {
        }

        public DbSet<FileSet> FileSets { get; set; }
                
        public DbSet<StoredFile> StoredFiles { get; set; }
        
        public DbSet<SharedFile> SharedFiles { get; set; }

        public DbSet<SharedFolder> SharedFolders { get; set; }

        public DbSet<SharedFileSet> SharedFileSets { get; set; }
    }
}
