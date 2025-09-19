using System.Globalization;
using System.Windows;
using ProyectoIntegradorS5.Views;
using ProyectoIntegradorS5.Controladores;

namespace ProyectoIntegradorS5
{
    public partial class MainWindow : Window
    {
        private readonly RecursoController recursoController;

        public MainWindow()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            InitializeComponent();

            recursoController = App.GetService<RecursoController>();

            MainContentGrid.Children.Add(new ProduccionView(recursoController));
        }

        private void OnProduccionClick(object sender, RoutedEventArgs e)
        {
            MainContentGrid.Children.Clear();
            MainContentGrid.Children.Add(new ProduccionView(recursoController));
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