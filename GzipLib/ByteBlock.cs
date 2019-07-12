using System;
using System.Threading;

namespace GzipLib
{
    /// <summary>
    /// Describes portion of data wich application work with.
    /// Can't load all file in memory
    /// </summary>
    public class ByteBlock
    {
        /// <summary>
        /// Count of added blocks
        /// </summary>
        private static int counter = 0;

        /// <summary>
        /// Index of current block. Uses for write into file with correct order of blocks.
        /// </summary>
        public int index;

        /// <summary>
        /// Block with data
        /// </summary>
        public byte[] data;

        /// <summary>
        /// Create block
        /// </summary>
        /// <param name="b">Portion of data</param>
        public ByteBlock(byte[] b)
        {
            Interlocked.Increment(ref counter);
            index = counter;
            data = new byte[b.Length];
            Array.Copy(b, data, b.Length);
        }
    }
}
