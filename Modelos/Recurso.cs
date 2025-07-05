using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoIntegradorS5.Modelos
{
    public class Recurso
    {
        public string Nombre { get; set; }
        public decimal CostoUnitario { get; set; }
        public int InventarioDisponible { get; set; }
        public int TiempoProduccionMinutos { get; set; }
        public List<Componente> Componentes { get; set; } = new();

        public override string ToString() => Nombre;
    }
}
