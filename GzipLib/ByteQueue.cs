using System.Collections.Generic;

namespace GzipLib
{
    /// <summary>
    /// Threadsafe queue of blocks.
    /// </summary>
    public class ByteQueue : Queue<ByteBlock>
    {
        /// <summary>
        /// Lock object
        /// </summary>
        readonly object _lock =new object ();

        /// <summary>
        /// Add block to queue
        /// </summary>
        /// <param name="b"></param>
        public void Enqueue(byte[] b)
        {
            lock(_lock)
            {
                base.Enqueue(new ByteBlock(b));
            }
        }

        /// <summary>
        /// Add block to queue
        /// </summary>
        /// <param name="block"></param>
        public void Enqueue(ByteBlock block)
        {
            lock(_lock)
            {
                base.Enqueue(block);
            }
        }

        /// <summary>
        /// Get block fromm queue
        /// </summary>
        /// <returns>Блок</returns>
        public ByteBlock Dequeue()
        {
            ByteBlock block;
            lock(_lock)
            {
                if (base.Count == 0)
                    return null;
                block = base.Dequeue();
            }
            return block;
        }

        /// <summary>
        /// Count of blocks in queue
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            int i;
            lock(_lock)
            {
                i = base.Count;
            }
            return i;
        }
    }
}
