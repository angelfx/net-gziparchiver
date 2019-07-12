using System;
using System.Threading;

namespace GzipLib.Threads
{
    /// <summary>
    /// Class for manage threads. Like a thread pool
    /// </summary>
    public class ThreadsManager : IDisposable
    {
        private int _size { get; set; }
        private int _tail { get; set; }
        private CustomThread[] _threads { get; set; }
        private int _count { get; set; }
        private bool _disposed { get; set; }

        /// <summary>
        /// Set maximum size of threads for compress task
        /// </summary>
        /// <param name="size"></param>
        public ThreadsManager(int size)
        {
            _size = size;
            _tail = -1;
            _count = 0;
            _disposed = false;
            _threads = new CustomThread[size];
        }

        /// <summary>
        /// Add parameterized thread to array
        /// </summary>
        /// <param name="start">Передаем функцию с параметрами</param>
        public void AddThread(ParameterizedThreadStart start)
        {
            _tail++;
            _count++;
            _threads[_tail] = new CustomThread(start, _tail);
            //_threads[_tail].IsBackground = true;
            //_threads[_tail].Name = "Поток " + _tail.ToString();
        }

        /// <summary>
        /// Add simple thread to array
        /// </summary>
        /// <param name="start">Передаем функцию</param>
        public void AddThread(ThreadStart start)
        {
            _tail++;
            _count++;
            _threads[_tail] = new CustomThread(start);
            //_threads[_tail].IsBackground = true;
            //_threads[_tail].Name = "Поток " + _tail.ToString();
        }

        /// <summary>
        /// Start threads
        /// </summary>
        /// <param name="data"></param>
        public void StartThreads()
        {
            for (int i = 0; i < _count; i++)
            {
                _threads[i].Start();
            }
        }

        /// <summary>
        /// Start threads with parameters
        /// </summary>
        /// <param name="data"></param>
        public void StartThreads(int cnt)
        {
            for (int i = 0; i < cnt; i++)
            {
                _threads[i].Start();
            }
        }

        /// <summary>
        /// Wait all threads
        /// </summary>
        public void WaitAll()
        {
            for (int i = 0; i < _count; i++)
            {
                _threads[i].Join();
                //_threads[i].Abort();
                //_threads[i] = null;
            }
            _count = 0;
            _tail = -1;
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                GC.SuppressFinalize(this);
                for (int i = 0; i < _count; i++)
                {
                    if (_threads[i] != null)
                        _threads[i].Abort();
                }
                this._disposed = true;
                _count = 0;
                _tail = -1;
            }
        }

        public bool IsEmpty()
        {
            return this._count == 0;
        }

        public int Count()
        {
            return this._count;
        }


    }
}
