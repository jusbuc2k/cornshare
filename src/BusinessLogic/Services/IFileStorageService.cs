using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Stores a file and assigns it a unique storage file name which is returned.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        string Put(System.IO.Stream stream);

        void Put(System.IO.Stream stream, string fileName);

        /// <summary>
        /// Returns a file stream by name.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        System.IO.Stream Get(string fileName);

        /// <summary>
        /// Gets a value that indicates if the file exists.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        bool Exists(string fileName);

        void Delete(string fileName);
    }
}
