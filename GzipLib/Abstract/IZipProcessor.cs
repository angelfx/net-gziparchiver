using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GzipLib.Abstract
{
    public interface IZipProcessor
    {
        /// <summary>
        /// Compressing data
        /// </summary>
        /// <param name="data">Array of bytes to compress</param>
        /// <returns>Compressed array of bytes</returns>
        byte[] Compress(byte[] data);

        /// <summary>
        /// Decompressing data
        /// </summary>
        /// <param name="data">Compressed array of bytes</param>
        /// <returns>Decompressed array of bytes</returns>
        byte[] Decompress(byte[] data);
    }
}
