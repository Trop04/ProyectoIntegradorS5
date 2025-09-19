using Microsoft.Msagl.Drawing;
using ProyectoIntegradorS5.Modelos;

namespace ProyectoIntegradorS5.Modelos
{
    public interface IVisualizadorGrafo
    {
        Graph GenerarGrafoProduccion(Recurso recurso, int cantidad);
    }
}