using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GzipLib.Abstract
{
    public interface IZipFactory : IDisposable
    {
        bool StartProcess();
    }
}
