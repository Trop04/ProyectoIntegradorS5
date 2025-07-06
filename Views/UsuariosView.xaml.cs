using System.Windows;
using System.Windows.Controls;
using ProyectoIntegradorS5.Modelos;
using ProyectoIntegradorS5.Servicios;
using System.Collections.ObjectModel;

namespace ProyectoIntegradorS5.Views
{
    public partial class UsuariosView : UserControl
    {
        private readonly UsuarioService usuarioService = new();

        public ObservableCollection<Usuario> Clientes { get; set; } = new();
        public ObservableCollection<Usuario> Empleados { get; set; } = new();

        public UsuariosView()
        {
            InitializeComponent();
            CargarDatos();
        }

        private async void CargarDatos()
        {
            var usuarios = await usuarioService.ObtenerTodosAsync();

            Clientes.Clear();
            Empleados.Clear();

            foreach (var usuario in usuarios)
            {
                if (usuario.Tipo == "Cliente")
                    Clientes.Add(usuario);
                else if (usuario.Tipo == "Empleado")
                    Empleados.Add(usuario);
            }

            dgClientes.ItemsSource = Clientes;
            dgEmpleados.ItemsSource = Empleados;
        }

        // CLIENTES

        private void OnAñadirCliente(object sender, RoutedEventArgs e)
        {
            var ventana = new UsuarioDetalleWindow("Cliente");
            if (ventana.ShowDialog() == true)
            {
                usuarioService.Agregar(ventana.Usuario);
                CargarDatos();
            }
        }

        private void OnEditarCliente(object sender, RoutedEventArgs e)
        {
            if (dgClientes.SelectedItem is not Usuario seleccionado)
            {
                MessageBox.Show("Selecciona un cliente primero.");
                return;
            }

            var ventana = new UsuarioDetalleWindow("Cliente", seleccionado);
            if (ventana.ShowDialog() == true)
            {
                usuarioService.Actualizar(ventana.Usuario);
                CargarDatos();
            }
        }

        private void OnEliminarCliente(object sender, RoutedEventArgs e)
        {
            if (dgClientes.SelectedItem is not Usuario seleccionado)
            {
                MessageBox.Show("Selecciona un cliente primero.");
                return;
            }

            var confirm = MessageBox.Show($"¿Eliminar a {seleccionado.NombreCompleto}?", "Confirmar", MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {
                usuarioService.Eliminar(seleccionado.Id);
                CargarDatos();
            }
        }

        private void OnBuscarCliente(object sender, RoutedEventArgs e)
        {
            var filtro = txtFiltroCliente.Text.ToLower();
            dgClientes.ItemsSource = string.IsNullOrWhiteSpace(filtro)
                ? Clientes
                : new ObservableCollection<Usuario>(Clientes.Where(u =>
                    u.NombreCompleto.ToLower().Contains(filtro) ||
                    u.Correo.ToLower().Contains(filtro)));
        }

        // EMPLEADOS

        private void OnAñadirEmpleado(object sender, RoutedEventArgs e)
        {
            var ventana = new UsuarioDetalleWindow("Empleado");
            if (ventana.ShowDialog() == true)
            {
                usuarioService.Agregar(ventana.Usuario);
                CargarDatos();
            }
        }

        private void OnEditarEmpleado(object sender, RoutedEventArgs e)
        {
            if (dgEmpleados.SelectedItem is not Usuario seleccionado)
            {
                MessageBox.Show("Selecciona un empleado primero.");
                return;
            }

            var ventana = new UsuarioDetalleWindow("Empleado", seleccionado);
            if (ventana.ShowDialog() == true)
            {
                usuarioService.Actualizar(ventana.Usuario);
                CargarDatos();
            }
        }

        private void OnEliminarEmpleado(object sender, RoutedEventArgs e)
        {
            if (dgEmpleados.SelectedItem is not Usuario seleccionado)
            {
                MessageBox.Show("Selecciona un empleado primero.");
                return;
            }

            var confirm = MessageBox.Show($"¿Eliminar a {seleccionado.NombreCompleto}?", "Confirmar", MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {
                usuarioService.Eliminar(seleccionado.Id);
                CargarDatos();
            }
        }

        private void OnBuscarEmpleado(object sender, RoutedEventArgs e)
        {
            var filtro = txtFiltroEmpleado.Text.ToLower();
            dgEmpleados.ItemsSource = string.IsNullOrWhiteSpace(filtro)
                ? Empleados
                : new ObservableCollection<Usuario>(Empleados.Where(u =>
                    u.NombreCompleto.ToLower().Contains(filtro) ||
                    u.Correo.ToLower().Contains(filtro)));
        }
    }
}
