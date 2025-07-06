using ProyectoIntegradorS5.Modelos;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProyectoIntegradorS5.Views
{
    public partial class UsuarioDetalleWindow : Window
    {
        public Usuario Usuario { get; private set; }
        private string tipo;

        public UsuarioDetalleWindow(string tipo, Usuario usuarioExistente = null)
        {
            InitializeComponent();
            this.tipo = tipo;
            Usuario = usuarioExistente ?? new Usuario { Tipo = tipo, FechaCreacion = DateTime.Now };

            Title = usuarioExistente == null ? $"Nuevo {tipo}" : $"Editar {tipo}";
            clientePanel.Visibility = tipo == "Cliente" ? Visibility.Visible : Visibility.Collapsed;
            empleadoPanel.Visibility = tipo == "Empleado" ? Visibility.Visible : Visibility.Collapsed;

            if (usuarioExistente != null)
            {
                txtNombre.Text = Usuario.NombreCompleto;
                txtCorreo.Text = Usuario.Correo;
                txtTelefono.Text = Usuario.Telefono;
                dpFechaNacimiento.SelectedDate = Usuario.FechaNacimiento;

                cmbEstado.SelectedIndex = Usuario.Estado == "Activo" ? 0 : 1;

                if (tipo == "Cliente")
                {
                    txtDireccion.Text = Usuario.Direccion;
                    txtCiudad.Text = Usuario.Ciudad;
                }
                else if (tipo == "Empleado")
                {
                    txtCargo.Text = Usuario.Cargo;
                    txtDepartamento.Text = Usuario.Departamento;
                    txtSalario.Text = Usuario.Salario.ToString();
                    txtHorario.Text = Usuario.Horario;
                }
            }
        }

        private void PART_Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            Calendar calendar = sender as Calendar;
            if (calendar != null)
            {
                DatePicker parentDatePicker = FindVisualParent<DatePicker>(calendar);

                if (parentDatePicker != null)
                {
                    parentDatePicker.IsDropDownOpen = false;
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) //Metodo de arrastrar, no cambiar
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }


        private T FindVisualParent<T>(System.Windows.DependencyObject child) where T : System.Windows.DependencyObject
        {
            var parent = System.Windows.Media.VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            if (parent is T typedParent) return typedParent;
            return FindVisualParent<T>(parent);
        }

        private void OnGuardar(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtCorreo.Text))
            {
                MessageBox.Show("Nombre y correo son obligatorios.");
                return;
            }

            Usuario.NombreCompleto = txtNombre.Text;
            Usuario.Correo = txtCorreo.Text;
            Usuario.Telefono = txtTelefono.Text;
            Usuario.FechaNacimiento = dpFechaNacimiento.SelectedDate ?? DateTime.MinValue;
            Usuario.Estado = (cmbEstado.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Activo";

            if (tipo == "Cliente")
            {
                Usuario.Direccion = txtDireccion.Text;
                Usuario.Ciudad = txtCiudad.Text;
            }
            else if (tipo == "Empleado")
            {
                Usuario.Cargo = txtCargo.Text;
                Usuario.Departamento = txtDepartamento.Text;
                Usuario.Horario = txtHorario.Text;
                if (decimal.TryParse(txtSalario.Text, out decimal salario))
                    Usuario.Salario = salario;
            }

            DialogResult = true;
            Close();
        }

        private void OnCancelar(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
