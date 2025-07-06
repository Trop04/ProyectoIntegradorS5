using System.Windows;
using System.Windows.Controls;
using ProyectoIntegradorS5.Modelos;
using ProyectoIntegradorS5.Servicios;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Linq;

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

        private void ExportarClientes_Click(object sender, RoutedEventArgs e)
        {
            ExportarCSV(Clientes, new[] { "NombreCompleto", "Correo", "Telefono", "Direccion", "Ciudad", "Estado", "FechaCreacion" });
        }

        private void ExportarEmpleados_Click(object sender, RoutedEventArgs e)
        {
            ExportarCSV(Empleados, new[] { "NombreCompleto", "Correo", "Telefono", "Cargo", "Departamento", "Salario", "Horario", "Estado" });
        }

        private void ExportarCSV(ObservableCollection<Usuario> lista, string[] propiedades)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = "Exportacion.csv"
            };

            if (dlg.ShowDialog() == true)
            {
                var sb = new StringBuilder();

                // Cabecera
                sb.AppendLine(string.Join(",", propiedades));

                foreach (var item in lista)
                {
                    var valores = propiedades.Select(prop =>
                    {
                        var propInfo = typeof(Usuario).GetProperty(prop);
                        if (propInfo != null)
                        {
                            var val = propInfo.GetValue(item);
                            if (val == null) return "";
                            if (val is DateTime dt) return dt.ToString("yyyy-MM-dd");
                            if (val is decimal dec) return dec.ToString("F2");
                            return val.ToString().Replace(",", " ");
                        }
                        return "";
                    });

                    sb.AppendLine(string.Join(",", valores));
                }

                File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Exportación completada.", "Exportar CSV", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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
