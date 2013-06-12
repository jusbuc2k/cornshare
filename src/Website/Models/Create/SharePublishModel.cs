using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class SharePublishModel
    {
        public int FileSetID { get; set; }
        public string Name { get; set; }
        public bool RequirePassword { get; set; }
        public string Password { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool AllowUpload { get; set; }
    }
}