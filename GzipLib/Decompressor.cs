using GzipLib.Abstract;
using GzipLib.Threads;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GzipLib
{
    /// <summary>
    /// Implements decompress algorithms using gzip
    /// </summary>
    public class Decompressor : ZipFactory, IZipFactory
    {
        public Decompressor(string inputFile, string outputDir, int numberOfCores = 0) : base(inputFile, outputDir, numberOfCores)
        {
            base.compressionMode = CompressionMode.Decompress;
        }

        public bool StartProcess()
        {
            this.DecompressParallel();

            return true;
        }

        public void ReadFromFile()
        {
            using (FileStream fStream = new FileStream(_inputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096 * 1024))
            {
                int count;
                byte[] buffer = null;

                //Work while application was not canceled
                while (!Canceled)
                {
                    if (byteQueue.Count() >= base._totalThreads)
                    {
                        while (byteQueue.Count() >= base._totalThreads)
                        {
                            zipProcessEvent.WaitOne(50);
                        }
                    }

                    byte[] lenFrame = new byte[4];
                    count = fStream.Read(lenFrame, 0, 4); //Read for first four bytes
                    if (count <= 0) break; //Exit from loop if end of file
                    var length = BitConverter.ToInt32(lenFrame, 0); //Get length of compressed block
                    buffer = new byte[length];
                    count = fStream.Read(buffer, 0, length); //Reading compressed block
                    if (count <= 0) break; //Exit from loop if end of file
                    byteQueue.Enqueue(buffer); //Add block to queue
                    blockReadEvent.Set();
                }

                base._finishRead = true;
            }
        }

        public void WriteToFile()
        {
            FileStream targetStream = CreateOutputSream();
            try
            {
                byte[] data;
                int nextBlockNumber = 1;

                while (!Canceled)
                {
                    if (outputBlocks.Count == 0 && IsWorkComplete == true)
                    {
                        break;
                    }

                    data = null;

                    //Get block by index
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

                    targetStream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            { throw ex; }
            finally
            {
                targetStream.Close();
            }
        }
        
        /// <summary>
        /// Parallel decompress
        /// </summary>
        protected void DecompressParallel()
        {
            //Create thread pool
            var tPool = new ThreadsManager(base._totalThreads);

            try
            {
                Thread readThread = new Thread(this.ReadFromFile);
                readThread.Start();
                Thread writeThread = new Thread(this.WriteToFile);
                writeThread.Start();

                for (int i = 0; i < base._totalThreads; i++)
                {
                    tPool.AddThread(DataProcessing);
                }
                tPool.StartThreads();

                tPool.WaitAll();

                IsWorkComplete = true;

                writeThread.Join();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                tPool.Dispose();
            }
        }
    }
}
