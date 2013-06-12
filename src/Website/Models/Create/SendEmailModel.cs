using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class SendEmailModel
    {
        public int FileSetID { get; set; }
        public bool RequirePassword { get; set; }
        public bool AllowUpload { get; set; }
        public DateTime ExpirationDate { get; set; }

        public string From { get; set; }
        public string To { get; set; }
        public string Body { get; set; }
        public string Link { get; set; }
    }
}