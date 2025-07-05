using System.Globalization;
using System.Windows;
using ProyectoIntegradorS5.Views;
using ProyectoIntegradorS5.Controladores;

namespace ProyectoIntegradorS5
{
    public partial class MainWindow : Window
    {
        private RecursoController vm = new();

        public MainWindow()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            InitializeComponent();

            // Cargar vista inicial: Producción
            MainContentGrid.Children.Add(new ProduccionView(vm));
        }

        private void OnProduccionClick(object sender, RoutedEventArgs e)
        {
            MainContentGrid.Children.Clear();
            MainContentGrid.Children.Add(new ProduccionView(vm));
        }

        private void OnUsuariosClick(object sender, RoutedEventArgs e)
        {
            MainContentGrid.Children.Clear();
            MainContentGrid.Children.Add(new UsuariosView());
        }

        private void OnVentasClick(object sender, RoutedEventArgs e)
        {
            MainContentGrid.Children.Clear();
            MainContentGrid.Children.Add(new VentasView());
        }
    }
}
