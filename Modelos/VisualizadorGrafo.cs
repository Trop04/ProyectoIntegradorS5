using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoIntegradorS5.Modelos
{
    public class VisualizadorGrafo
    {
        private readonly IAlgoritmoCosto planificador;

        public VisualizadorGrafo(IAlgoritmoCosto planificador)
        {
            this.planificador = planificador;
        }

        public Microsoft.Msagl.Drawing.Graph GenerarGrafoProduccion(Recurso recurso, int cantidad)
        {
            var grafo = new Microsoft.Msagl.Drawing.Graph("grafoProduccion");
            AgregarNodos(grafo, recurso, null, cantidad);
            return grafo;
        }

        private void AgregarNodos(Microsoft.Msagl.Drawing.Graph grafo, Recurso recurso, string padre, int cantidad)
        {
            string nodoId = $"{recurso.Nombre}\nCantidad: {cantidad}\nCosto: {planificador.CalcularCosto(recurso, cantidad).ToString("C", new CultureInfo("en-US"))}";
            grafo.AddNode(nodoId);
            if (padre != null)
                grafo.AddEdge(padre, nodoId);

            foreach (var comp in recurso.Componentes)
            {
                AgregarNodos(grafo, comp.Recurso, nodoId, comp.CantidadNecesaria * cantidad);
            }
        }
    }
}
