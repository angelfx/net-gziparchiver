using System;
using System.Threading;

namespace GzipLib.Threads
{
    /// <summary>
    /// Implements class for work with thread
    /// </summary>
    public class CustomThread : IDisposable
    {
        /// <summary>
        /// Thread
        /// </summary>
        private Thread _thread { get; set; }

        /// <summary>
        /// Parameterized thread
        /// </summary>
        private bool _parameterized { get; set; }

        /// <summary>
        /// Index of thread
        /// </summary>
        private int _index { get; set; }

        private bool _disposed;

        /// <summary>
        /// State of thread
        /// </summary>
        public bool IsAlive
        {
            get
            {
                return this._thread.IsAlive;
            }
        }

        public bool IsBackground
        {
            get
            {
                return this._thread.IsBackground;
            }
            set
            {
                this.IsBackground = value;
            }
        }

        public int Index { get { return _index; } }

        public CustomThread(ThreadStart start)
        {
            _thread = new Thread(start);
            _parameterized = false;
        }

        public CustomThread(ParameterizedThreadStart start, int index)
        {
            _thread = new Thread(start);
            _thread.Name = "Поток " + index.ToString();
            _index = index;
            _parameterized = true;
        }

        public void Abort()
        {
            _thread.Abort();
        }

        public void Join()
        {
            _thread.Join();
        }

        public void Start()
        {
            if (_parameterized)
                _thread.Start(_index);
            else
                _thread.Start();
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                GC.SuppressFinalize(this);
                if (this._thread != null)
                {
                    this._thread.Abort();
                    this._thread = null;
                }

                this._disposed = true;
            }
        }
    }
}
