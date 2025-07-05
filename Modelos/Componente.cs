using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoIntegradorS5.Modelos
{
    public class Componente
    {
        public Recurso Recurso { get; set; }
        public int CantidadNecesaria { get; set; }
        public override string ToString() => $"{Recurso?.Nombre} x{CantidadNecesaria}";
    }
}
