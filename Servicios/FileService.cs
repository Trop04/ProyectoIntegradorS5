using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ProyectoIntegradorS5.Servicios
{
    public class FileService : IFileService
    {
        public void WriteAllText(string path, string content) => File.WriteAllText(path, content);
        public string ReadAllText(string path) => File.ReadAllText(path);
        public bool Exists(string path) => File.Exists(path);
    }
}
