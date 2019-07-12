using GzipLib.Abstract;
using GzipLib.GZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GzipLib
{
    public abstract class ZipFactory : IDisposable
    {
        protected static readonly AutoResetEvent zipProcessEvent = new AutoResetEvent(false);
        protected static readonly AutoResetEvent blockReadEvent = new AutoResetEvent(false);

        /// <summary>
        /// Input file for compress or decompress
        /// </summary>
        protected string _inputFile;

        /// <summary>
        /// Output file
        /// </summary>
        protected string _outputFile;

        /// <summary>
        /// Buffer size. 1 Mb
        /// </summary>
        protected static int bufferSize = 1024 * 1024 * 1;

        /// <summary>
        /// Minimum file size to process
        /// </summary>
        protected static int minFileSize = 1024 * 1024 * 25;

        /// <summary>
        /// Queue of blocks for compress or decompress
        /// </summary>
        protected static ByteQueue byteQueue;

        /// <summary>
        /// Output blocks. Key st order of blocks for output file
        /// </summary>
        public Dictionary<int, byte[]> outputBlocks = new Dictionary<int, byte[]>();

        /// <summary>
        /// Quantity of cores. Can set in constructor manually
        /// </summary>
        private static int _numberOfCores;

        /// <summary>
        /// Threads per core
        /// </summary>
        private static int _threadsPerCore = 2;

        /// <summary>
        /// Tell that read was finished
        /// </summary>
        protected bool _finishRead;

        /// <summary>
        /// Queue is empty
        /// </summary>
        protected static bool IsWorkComplete = false;

        /// <summary>
        /// Process canceled by user
        /// </summary>
        protected static bool Canceled = false;

        /// <summary>
        /// Count of threads for process
        /// </summary>
        protected int _totalThreads
        {
            get
            {
                return _numberOfCores * _threadsPerCore;
            }
        }

        /// <summary>
        /// Compresssion mode
        /// </summary>
        protected CompressionMode compressionMode;

        /// <summary>
        /// Processor for compress or decompress
        /// </summary>
        protected IZipProcessor zipProcessor;

        private bool _disposed = false;

        public ZipFactory(string inputFile, string outputFile, int numberOfCores = 0)
        {
            byteQueue = new ByteQueue();
            _inputFile = inputFile;
            _outputFile = outputFile;

            if (numberOfCores == 0)
                _numberOfCores = Environment.ProcessorCount;
            else
                _numberOfCores = numberOfCores;

            zipProcessor = new GZipProcessor();
        }

        /// <summary>
        /// Method for compressing or decompress data. Starts in thread.
        /// Get data from queue and process. Result blocks adding to outputBlocks by index
        /// </summary>
        /// <param name="data"></param>
        protected void DataProcessing(object data)
        {
            //var index = (int)data;

            ByteBlock block = null;

            while (!IsWorkComplete && !Canceled)
            {
                if (this._finishRead && byteQueue.Count() == 0)
                    break;
                else
                {
                    byte[] b;
                    lock (outputBlocks)
                    {
                        if (outputBlocks.Count > _totalThreads)
                        {
                            continue;
                        }
                    }
                    if (byteQueue.Count() == 0)
                    {
                        blockReadEvent.WaitOne(20);
                        continue;
                    }
                    block = byteQueue.Dequeue();

                    if (block == null) continue;

                    if (compressionMode == CompressionMode.Compress)
                        b = zipProcessor.Compress(block.data);
                    else
                        b = zipProcessor.Decompress(block.data);

                    if (b.Length > 0)
                    {
                        lock (outputBlocks)
                        {
                            outputBlocks.Add(block.index, b);
                            zipProcessEvent.Set();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create output stream
        /// </summary>
        /// <returns></returns>
        protected FileStream CreateOutputSream()
        {
            FileStream targetStream = null;

            try
            {
                targetStream = new FileStream(_outputFile, FileMode.Truncate);
            }
            catch (System.IO.FileNotFoundException)
            {
                targetStream = new FileStream(_outputFile, FileMode.CreateNew);
            }

            return targetStream;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            Canceled = true;
            if (disposing)
            {
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
