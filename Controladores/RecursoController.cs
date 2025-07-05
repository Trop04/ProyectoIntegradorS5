using ProyectoIntegradorS5.Modelos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ProyectoIntegradorS5.Controladores
{
    public class RecursoController
    {
        private readonly IAlgoritmoCosto planificador;
        private readonly VisualizadorGrafo visualizador;

        public List<Recurso> Recursos { get; private set; } = new();
        public Recurso RecursoSeleccionado { get; set; }
        public int CantidadProduccion { get; set; }

        public RecursoController()
        {
            planificador = new PlanificadorBasico();
            visualizador = new VisualizadorGrafo(planificador);
        }

        public void AgregarRecurso(Recurso recurso) => Recursos.Add(recurso);

        public void EditarRecurso(Recurso original, Recurso actualizado)
        {
            var index = Recursos.IndexOf(original);
            if (index >= 0)
                Recursos[index] = actualizado;
        }

        public void EliminarRecurso(Recurso recurso)
        {
            Recursos.Remove(recurso);
            foreach (var r in Recursos)
                r.Componentes.RemoveAll(c => c.Recurso == recurso);
        }

        public Microsoft.Msagl.Drawing.Graph GenerarGrafo(out string resumen)
        {
            var grafo = visualizador.GenerarGrafoProduccion(RecursoSeleccionado, CantidadProduccion);
            var puedeProducir = ((PlanificadorBasico)planificador).PuedeProducir(RecursoSeleccionado, CantidadProduccion);

            if (!puedeProducir)
            {
                resumen = "No se puede producir la cantidad deseada por restricciones de inventario.";
                return grafo;
            }

            var costo = planificador.CalcularCosto(RecursoSeleccionado, CantidadProduccion);
            var tiempo = planificador.CalcularTiempoProduccion(RecursoSeleccionado, CantidadProduccion);

            resumen = $"Costo total: {costo.ToString("C", new CultureInfo("en-US"))}\nTiempo total estimado: {tiempo} minutos";
            return grafo;
        }

        public void Guardar(string ruta)
        {
            var opciones = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(Recursos, opciones);
            File.WriteAllText(ruta, json);
        }

        public void Cargar(string ruta)
        {
            if (!File.Exists(ruta))
                return;

            var json = File.ReadAllText(ruta);
            var cargados = JsonSerializer.Deserialize<List<Recurso>>(json);
            if (cargados != null)
            {
                Recursos = cargados;

                foreach (var recurso in Recursos)
                {
                    foreach (var componente in recurso.Componentes)
                    {
                        var refRecurso = Recursos.FirstOrDefault(r => r.Nombre == componente.Recurso?.Nombre);
                        if (refRecurso != null)
                            componente.Recurso = refRecurso;
                    }
                }
            }
        }
    }
}
