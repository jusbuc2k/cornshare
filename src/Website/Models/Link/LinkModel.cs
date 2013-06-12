using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class LinkModel
    {
        public string ShareToken { get; set; }
        public string FileSetName { get; set; }
        public bool PasswordRequired { get; set; }
        public bool PasswordIsValid { get; set; }
        public string Password { get; set; }
        public bool AllowUpload { get; set; }
        public IEnumerable<DownloadModel> Files { get; set; }
    }
}