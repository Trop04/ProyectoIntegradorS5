using Microsoft.Extensions.DependencyInjection;
using ProyectoIntegradorS5.Controladores;
using ProyectoIntegradorS5.Modelos;
using ProyectoIntegradorS5.Servicios;

namespace ProyectoIntegradorS5.Servicios
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            // loogica
            services.AddScoped<IAlgoritmoCosto, PlanificadorBasico>();
            services.AddScoped<IVisualizadorGrafo, VisualizadorGrafo>();

            // servicios
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<UsuarioService>();

            // controllers
            services.AddScoped<RecursoController>();

            return services;
        }
    }
}