using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoIntegradorS5.Servicios
{
    public interface IFileService
    {
        void WriteAllText(string path, string content);
        string ReadAllText(string path);
        bool Exists(string path);
    }
}
