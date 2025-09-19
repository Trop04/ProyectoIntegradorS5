using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoIntegradorS5.Modelos
{
    public interface IAlgoritmoCosto
    {
        decimal CalcularCosto(Recurso recurso, int cantidad);
        int CalcularTiempoProduccion(Recurso recurso, int cantidad);
        bool PuedeProducir(Recurso recurso, int cantidad);
    }
}
