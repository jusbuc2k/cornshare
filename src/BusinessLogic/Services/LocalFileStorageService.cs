using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        public LocalFileStorageService(string rootPath)
        {
            this.RootPath = rootPath;
        }

        public string RootPath { get; set; }

        #region IFileStorageService Members

        public string Put(System.IO.Stream stream)
        {
            var fileName = System.Guid.NewGuid().ToString();
            this.Put(stream, fileName);
            return fileName;
        }

        public void Put(System.IO.Stream stream, string fileName)
        {            
            var prefix = fileName.Substring(0, 2);
            var folder = new System.IO.DirectoryInfo(System.IO.Path.Combine(this.RootPath, prefix));

            if (!folder.Exists)
                folder.Create();

            var buffer = new byte[4096];
            int count = 0;

            using (var outFile = System.IO.File.Open(System.IO.Path.Combine(this.RootPath, prefix, fileName), System.IO.FileMode.CreateNew))
            {
                while (true)
                {
                    count = stream.Read(buffer, 0, buffer.Length);
                    if (count <= 0)
                        break;
                    outFile.Write(buffer, 0, count);
                }
            }            
        }

        public System.IO.Stream Get(string fileName)
        {
            var prefix = fileName.Substring(0, 2);
            var file = new System.IO.FileInfo(System.IO.Path.Combine(this.RootPath, prefix, fileName));

            if (file.Exists)
            {
                return file.OpenRead();
            }
            else
            {
                throw new Exception("file not found");
            }
        }

        public bool Exists(string fileName)
        {
            var prefix = fileName.Substring(0, 2);
            var file = new System.IO.FileInfo(System.IO.Path.Combine(this.RootPath, prefix, fileName));

            return file.Exists;
        }

        public void Delete(string fileName)
        {
            var prefix = fileName.Substring(0, 2);
            var file = new System.IO.FileInfo(System.IO.Path.Combine(this.RootPath, prefix, fileName));

            if (file.Exists)
            {
                file.Delete();
            }
        }

        #endregion
    }
}
