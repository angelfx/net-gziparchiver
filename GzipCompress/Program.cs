using GzipCompress.Utils;
using GzipLib.Abstract;
using System;


namespace GzipCompress
{
    class Program
    {
        public static bool Error = false;

        static void Main(string[] args)
        {
            LogManager log = new LogManager("zip");
            IZipFactory zipFactory = null;
            log.StartBlock("Work");

            Console.CancelKeyPress += (sender, a) =>
            {
                zipFactory.Dispose();
                Console.WriteLine("Application was stoped. Press any key...");
                log.WriteError("Process was canceled by user");
                Console.ReadKey();
                Environment.Exit(1);
            };

            try
            {
                log.WriteMessage("Parsing arguments...");
                Options.IntitalizeArgs(args);
                log.WriteMessage($"Mode: {Options.CompressionMode.ToString()}; Input file: {Options.InputFile}; Output file: {Options.OutputFile}; CPU Cores: {Options.NumberOfCores}");

                zipFactory = ResolverFactory.CreateFactory();
                if (zipFactory == null)
                {
                    log.WriteError("Processor is null");
                    Environment.Exit(1);
                }

                Console.WriteLine($"{Options.CompressionMode.ToString()}. Wait...");
                int begin = System.Environment.TickCount;
                log.WriteMessage("Start process");
                zipFactory.StartProcess();
                log.WriteMessage($"Process finished. Elapsed time {System.Environment.TickCount - begin} milliseconds");
            }
            catch (Exception ex)
            {
                log.WriteError(ex);
                Error = true;
                GC.Collect();
            }
            finally
            {
                if (zipFactory != null)
                    zipFactory.Dispose();
                log.StopBlock("Work");
            }

            if (Error)
                Environment.Exit(1);
            else
                Environment.Exit(0);
        }

    }
}
