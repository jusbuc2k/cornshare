using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class ShareCreateModel
    {
        public int FileSetID { get; set; }
        public string Name { get; set; }
        public bool IsNew { get; set; }
        public IEnumerable<StoredFileModel> Files {get;set;}        
    }


}