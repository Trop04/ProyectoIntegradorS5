using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoIntegradorS5.Modelos
{
    public class CentroTrabajo
    {
        public string Nombre { get; set; }
        public int CapacidadProduccionPorHora { get; set; }
        public decimal CostoHora { get; set; }
    }
}
