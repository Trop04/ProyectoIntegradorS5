using ProyectoIntegradorS5.Modelos;
using ProyectoIntegradorS5.Servicios;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace ProyectoIntegradorS5.Controladores
{
    public class RecursoController
    {
        private readonly IAlgoritmoCosto planificador;
        private readonly IVisualizadorGrafo visualizador;
        private readonly IFileService fileService;

        public List<Recurso> Recursos { get; private set; } = new();
        public Recurso RecursoSeleccionado { get; set; }
        public int CantidadProduccion { get; set; }

        public RecursoController(
            IAlgoritmoCosto planificador,
            IVisualizadorGrafo visualizador,
            IFileService fileService)
        {
            this.planificador = planificador ?? throw new ArgumentNullException(nameof(planificador));
            this.visualizador = visualizador ?? throw new ArgumentNullException(nameof(visualizador));
            this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
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
            bool puedeProducir;
            if (planificador is PlanificadorBasico planificadorBasico)
            {
                puedeProducir = planificadorBasico.PuedeProducir(RecursoSeleccionado, CantidadProduccion);
            }
            else
            {
                // Fallback 
                puedeProducir = true;
            }

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
            fileService.WriteAllText(ruta, json);
        }

        public void Cargar(string ruta)
        {
            if (!fileService.Exists(ruta))
                return;

            var json = fileService.ReadAllText(ruta);
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