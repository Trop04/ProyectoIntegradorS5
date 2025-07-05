using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoIntegradorS5.Modelos
{
    public class PlanificadorBasico : IAlgoritmoCosto
    {
        public decimal CalcularCosto(Recurso recurso, int cantidad)
        {
            if (recurso.Componentes == null || recurso.Componentes.Count == 0)
                return recurso.CostoUnitario * cantidad;

            decimal costo = 0;
            foreach (var comp in recurso.Componentes)
            {
                costo += CalcularCosto(comp.Recurso, comp.CantidadNecesaria * cantidad);
            }
            return costo + recurso.CostoUnitario * cantidad;
        }

        public int CalcularTiempoProduccion(Recurso recurso, int cantidad)
        {
            if (recurso.Componentes == null || recurso.Componentes.Count == 0)
                return recurso.TiempoProduccionMinutos * cantidad;

            int tiempo = 0;
            foreach (var comp in recurso.Componentes)
            {
                tiempo += CalcularTiempoProduccion(comp.Recurso, comp.CantidadNecesaria * cantidad);
            }
            return tiempo + recurso.TiempoProduccionMinutos * cantidad;
        }

        public bool PuedeProducir(Recurso recurso, int cantidad)
        {
            if (recurso.Componentes == null || recurso.Componentes.Count == 0)
                return recurso.InventarioDisponible >= cantidad;

            foreach (var comp in recurso.Componentes)
            {
                if (!PuedeProducir(comp.Recurso, comp.CantidadNecesaria * cantidad))
                    return false;
            }
            return true;
        }
    }
}
