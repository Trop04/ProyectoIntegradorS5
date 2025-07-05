using ProyectoIntegradorS5.Modelos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoIntegradorS5.Modelos
{
    public class Planificador
    {
        /**
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
            tiempo += recurso.TiempoProduccionMinutos * cantidad;
            return tiempo;
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

        public Microsoft.Msagl.Drawing.Graph GenerarGrafoProduccion(Recurso recurso, int cantidad)
        {
            var grafo = new Microsoft.Msagl.Drawing.Graph("grafoProduccion");
            AgregarNodos(grafo, recurso, null, cantidad);
            return grafo;
        }

        private void AgregarNodos(Microsoft.Msagl.Drawing.Graph grafo, Recurso recurso, string padre, int cantidad)
        {
            string nodoId = $"{recurso.Nombre}\nCantidad: {cantidad}\nCosto: {CalcularCosto(recurso, cantidad).ToString("C", new CultureInfo("en-US"))}";
            grafo.AddNode(nodoId);
            if (padre != null)
                grafo.AddEdge(padre, nodoId);

            foreach (var comp in recurso.Componentes)
            {
                AgregarNodos(grafo, comp.Recurso, nodoId, comp.CantidadNecesaria * cantidad);
            }
        }

        public Dictionary<CentroTrabajo, List<(Recurso, int)>> AsignarRecursosACentros(List<CentroTrabajo> centros, Recurso producto, int cantidad)
        {
            var asignacion = new Dictionary<CentroTrabajo, List<(Recurso, int)>>();
            var recursos = ObtenerRecursosNecesarios(producto, cantidad);

            foreach (var recurso in recursos)
            {
                var mejorCentro = centros.OrderBy(c => c.CostoHora).First();
                if (!asignacion.ContainsKey(mejorCentro))
                    asignacion[mejorCentro] = new();
                asignacion[mejorCentro].Add(recurso);
            }

            return asignacion;
        }

        private List<(Recurso, int)> ObtenerRecursosNecesarios(Recurso recurso, int cantidad)
        {
            var lista = new List<(Recurso, int)>();
            if (recurso.Componentes == null || recurso.Componentes.Count == 0)
            {
                lista.Add((recurso, cantidad));
                return lista;
            }

            foreach (var comp in recurso.Componentes)
            {
                lista.AddRange(ObtenerRecursosNecesarios(comp.Recurso, comp.CantidadNecesaria * cantidad));
            }
            return lista;
        }
                **/
    }

}