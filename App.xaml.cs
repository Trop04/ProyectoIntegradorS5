using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProyectoIntegradorS5.Servicios;
using System;
using System.Globalization;
using System.Windows;

namespace ProyectoIntegradorS5
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;
        private IHost _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

            base.OnStartup(e);

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.ConfigureServices();
                })
                .Build();

            _serviceProvider = _host.Services;

            var mainWindow = new MainWindow();

            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host?.Dispose();
            base.OnExit(e);
        }

        public static T GetService<T>() where T : class
        {
            return ((App)Current)._serviceProvider.GetRequiredService<T>();
        }
    }
}