using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class StoredFileModel
    {
        public string Filename { get; set; }
        public int FileID { get; set; }
        public string Length { get; set; }
    }
}