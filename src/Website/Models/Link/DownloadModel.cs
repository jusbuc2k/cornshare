using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class DownloadModel 
    {
        public string ShareToken { get; set; }
        public int FileID { get; set; }
        public string FileName { get; set; }
        public string Length { get; set; }        
    }    
}