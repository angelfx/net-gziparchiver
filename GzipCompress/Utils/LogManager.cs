using System;
using System.IO;
using System.Text;

namespace GzipCompress.Utils
{
    /// <summary>
    /// Iplements logic for work with application logs
    /// </summary>
    public class LogManager
    {
        /// <summary>
        /// File name
        /// </summary>
        private readonly string _Log = "LogHelper";

        /// <summary>
        /// Created date
        /// </summary>
        private readonly string _Created = "";

        /// <summary>
        /// Application folder
        /// </summary>
        private readonly string _Dir = "";

        /// <summary>
        /// Extension of file
        /// </summary>
        private const string _Ext = "log";

        /// <summary>
        /// Full name of file
        /// </summary>
        private readonly string _LogFile = "";

        /// <summary>
        /// Defaualt constrcutor. Set initial data
        /// </summary>
        public LogManager()
        {
            _Created = DateTime.Now.ToString("yyyy_MM_dd");
            _Dir = Path.Combine(Environment.CurrentDirectory, "Logs");
            if (!Directory.Exists(_Dir))
                Directory.CreateDirectory(_Dir);
            _LogFile = Path.Combine(_Dir, string.Format($"{_Created}_{_Log}.{_Ext}"));
        }

        /// <summary>
        /// Set log file name
        /// </summary>
        /// <param name="log"></param>
        public LogManager(string log)
        {
            _Log = log;
            _Created = DateTime.Now.ToString("yyyy_MM_dd");
            _Dir = Path.Combine(Environment.CurrentDirectory, "Logs");
            if (!Directory.Exists(_Dir))
                Directory.CreateDirectory(_Dir);
            _LogFile = Path.Combine(_Dir, string.Format($"{_Created}_{_Log}.{_Ext}"));
        }

        /// <summary>
        /// Set log file name and path to store logs
        /// </summary>
        /// <param name="log"></param>
        public LogManager(string log, string path)
        {
            _Log = log;
            _Created = DateTime.Now.ToString("yyyy_MM_dd");
            _Dir = Path.Combine(path == "" ? Environment.CurrentDirectory : path, "Logs");
            if (!Directory.Exists(_Dir))
                Directory.CreateDirectory(_Dir);
            _LogFile = Path.Combine(_Dir, string.Format($"{_Created}_{_Log}.{_Ext}"));
        }

        /// <summary>
        /// WRite simple message
        /// </summary>
        /// <param name="message"></param>
        public void WriteMessage(string message)
        {
            using (StreamWriter fileSync = new StreamWriter(new FileStream(_LogFile, System.IO.FileMode.Append)))
            {
                fileSync.WriteLine("{0}: {1}", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), message);
                fileSync.Flush();
                //fileSync.Close();
            }
        }

        /// <summary>
        /// Start block. It is the logic block in the log file.
        /// Example: "Start sync", "Start compress" and etc.
        /// </summary>
        /// <param name="block">Block name</param>
        public void StartBlock(string block)
        {
            using (StreamWriter fileSync = new StreamWriter(new FileStream(_LogFile, System.IO.FileMode.Append)))
            {
                fileSync.WriteLine("{0}: ******* Sratr {1} *******", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), block);
                fileSync.Flush();
            }
        }

        /// <summary>
        /// Stop block. Close the logic block in the log file.
        /// </summary>
        /// <param name="block"></param>
        public void StopBlock(string block)
        {
            using (StreamWriter fileSync = new StreamWriter(new FileStream(_LogFile, System.IO.FileMode.Append)))
            {
                fileSync.WriteLine("{0}: ******* Stop {1} *******", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), block);
                fileSync.Flush();
            }
        }

        /// <summary>
        /// Write custom error in the log file
        /// </summary>
        /// <param name="message">Mesage</param>
        /// <param name="stackTrace">Stack trace</param>
        /// <param name="innerExeption">Inner exeption message</param>
        public void WriteError(string message, string stackTrace = "", string innerExeption = "")
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine($"Error: {message}");
            if (!string.IsNullOrEmpty(stackTrace))
                str.AppendLine($"Stack trace: {stackTrace}");
            if (!string.IsNullOrEmpty(innerExeption))
                str.AppendLine($"Inner exception: {innerExeption}");
            using (StreamWriter fileSync = new StreamWriter(new FileStream(_LogFile, System.IO.FileMode.Append)))
            {
                fileSync.WriteLine("{0}: Exception", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                fileSync.WriteLine(str.ToString());
                fileSync.Flush();
            }
        }

        /// <summary>
        /// Write exeption
        /// Maybe make this function recrusive for inner expetions?
        /// </summary>
        /// <param name="ex">Exeption object</param>
        public void WriteError(Exception ex)
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine($"Error: {ex.Message}");
            str.AppendLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
                str.AppendLine($"Inner exception. Type: {ex.InnerException.GetType().ToString()}. Error: {ex.InnerException.Message}. Stack trace: {ex.InnerException}");
            using (StreamWriter fileSync = new StreamWriter(new FileStream(_LogFile, System.IO.FileMode.Append)))
            {
                fileSync.WriteLine("{0}: Exception =>", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                fileSync.WriteLine(str.ToString());
                fileSync.Flush();
            }
        }
    }
}
