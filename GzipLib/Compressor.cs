using GzipLib.Abstract;
using GzipLib.Threads;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Text;

namespace GzipLib
{
    /// <summary>
    /// Implements compress algorithms using gzip
    /// </summary>
    public class Compressor : ZipFactory, IZipFactory
    {
        /// <summary>
        /// Set initial parameters
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputDir"></param>
        /// <param name="numberOfCores"></param>
        public Compressor(string inputFile, string outputDir, int numberOfCores = 0) : base(inputFile, outputDir, numberOfCores)
        {
            base.compressionMode = CompressionMode.Compress;
            base._finishRead = false;
        }

        /// <summary>
        /// Start compress
        /// </summary>
        /// <returns></returns>
        public bool StartProcess()
        {
            FileInfo file = new FileInfo(_inputFile);
            //If file size lower than minimum file size than compress entire all file
            if (file.Length < minFileSize)
            {
                //Read all file to RAM
                using (FileStream fStream = new FileStream(_inputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    FileStream targetStream = CreateOutputSream();
                    try
                    {
                        byte[] buffer = new byte[fStream.Length];
                        fStream.Read(buffer, 0, buffer.Length);
                        var compressed = zipProcessor.Compress(buffer);

                        //Write length of compressed block into first four bytes
                        int lenFrame = compressed.Length;
                        targetStream.WriteByte((byte)(lenFrame & 0xff));
                        targetStream.WriteByte((byte)((lenFrame >> 8) & 0xff));
                        targetStream.WriteByte((byte)((lenFrame >> 16) & 0xff));
                        targetStream.WriteByte((byte)((lenFrame >> 24) & 0xff));

                        targetStream.Write(compressed, 0, compressed.Length);
                    }
                    finally
                    {
                        targetStream.Close();
                    }
                }
            }
            else
            {
                this.CompressParallel(); //Parallel compress
            }

            return true;
        }

        /// <summary>
        /// Read from file. Starts run thread
        /// </summary>
        private void ReadFromFile()
        {
            using (FileStream fStream = new FileStream(_inputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                int count;
                byte[] buffer = new byte[bufferSize];
                //While file is not end or application is not canceled
                while (fStream.Position < fStream.Length || !Canceled)
                {
                    count = fStream.Read(buffer, 0, buffer.Length);
                    if (count <= 0)
                        break;
                    //Wait if count of blocks in queue greater than available threads
                    if (byteQueue.Count() >= base._totalThreads)
                    {
                        while (byteQueue.Count() >= base._totalThreads)
                        {
                            zipProcessEvent.WaitOne(50);
                        }
                    }

                    if (count < bufferSize)
                    {
                        var b = new byte[count];
                        Array.Copy(buffer, b, count);
                        buffer = b;
                    }
                    byteQueue.Enqueue(buffer);
                    //buffer = new byte[bufferSize];
                    blockReadEvent.Set();
                }

                base._finishRead = true;
            }
        }

        /// <summary>
        /// Write into file. Starts in thread
        /// </summary>
        private void WriteToFile()
        {
            FileStream targetStream = CreateOutputSream();
            try
            {
                byte[] data;
                int nextBlockNumber = 1;
                while (!Canceled)
                {
                    data = null;
                    if (outputBlocks.Count == 0 && IsWorkComplete == true)
                    {
                        break;
                    }

                    lock (byteQueue)
                    {
                        if (outputBlocks.ContainsKey(nextBlockNumber))
                        {
                            data = outputBlocks[nextBlockNumber];
                            outputBlocks.Remove(nextBlockNumber);
                            nextBlockNumber++;
                        }
                    }
                    if (data == null)
                    {
                        zipProcessEvent.WaitOne(20);
                        continue;
                    }

                    //While compressing by blocks writing length of each compressed block
                    //In first four bytes before the block saving length of block
                    int lenFrame = data.Length;
                    targetStream.WriteByte((byte)(lenFrame & 0xff));
                    targetStream.WriteByte((byte)((lenFrame >> 8) & 0xff));
                    targetStream.WriteByte((byte)((lenFrame >> 16) & 0xff));
                    targetStream.WriteByte((byte)((lenFrame >> 24) & 0xff));

                    targetStream.Write(data, 0, data.Length);
                   
                }
            }
            finally
            {
                targetStream.Close();
            }
        }

        /// <summary>
        /// Parallel compressing
        /// </summary>
        protected void CompressParallel()
        {
            //Create pool of threads
            var tPool = new ThreadsManager(base._totalThreads);

            try
            {
                //Thread for reading
                Thread readThread = new Thread(this.ReadFromFile);
                readThread.Start();
                //Thread for writing
                Thread writeThread = new Thread(this.WriteToFile);
                writeThread.Start();

                //Add threads
                for (int i = 0; i < base._totalThreads; i++)
                {
                    tPool.AddThread(DataProcessing);
                }

                //Start threads
                tPool.StartThreads();

                //Waiting
                tPool.WaitAll();

                IsWorkComplete = true;

                //Wait finish write
                writeThread.Join();
            }
            finally
            {
                tPool.Dispose();
            }
        }
    }
}
