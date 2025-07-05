using Microsoft.Msagl.GraphViewerGdi;
using ProyectoIntegradorS5.Controladores;
using ProyectoIntegradorS5.Modelos;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;

namespace ProyectoIntegradorS5.Views
{
    public partial class ProduccionView : UserControl
    {
        private readonly RecursoController controller = new();
        private Recurso recursoEnEdicion = null;

        public ProduccionView(RecursoController vm)
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            InitializeComponent();
            controller = vm;
            /**
            var tabla = new Recurso { Nombre = "Tabla", CostoUnitario = 50, InventarioDisponible = 100, TiempoProduccionMinutos = 10 };
            var patas = new Recurso { Nombre = "Patas", CostoUnitario = 20, InventarioDisponible = 200, TiempoProduccionMinutos = 5 };
            var mesa = new Recurso
            {
                Nombre = "Mesa",
                CostoUnitario = 0,
                TiempoProduccionMinutos = 15,
                Componentes = new List<Componente> {
                    new Componente { Recurso = tabla, CantidadNecesaria = 1 },
                    new Componente { Recurso = patas, CantidadNecesaria = 4 }
                }
            };

            controller.AgregarRecurso(tabla);
            controller.AgregarRecurso(patas);
            controller.AgregarRecurso(mesa);
            **/
            cmbRecursos.ItemsSource = controller.Recursos;
            cmbComponentes.ItemsSource = controller.Recursos;
            lstRecursos.ItemsSource = controller.Recursos;
        }


        private void OnPlanificar(object sender, RoutedEventArgs e)
        {
            controller.RecursoSeleccionado = cmbRecursos.SelectedItem as Recurso;
            controller.CantidadProduccion = int.TryParse(txtCantidad.Text, out var cantidad) ? cantidad : 1;

            var grafo = controller.GenerarGrafo(out string resumen);
            var viewer = new GViewer { Graph = grafo };
            graphViewerHost.Child = viewer;
            MessageBox.Show(resumen);
        }

        private void OnAgregarRecurso(object sender, RoutedEventArgs e)
        {
            string nombre = txtNuevoNombre.Text;
            decimal.TryParse(txtNuevoCosto.Text, out var costo);
            int.TryParse(txtNuevoInventario.Text, out var inventario);
            int.TryParse(txtNuevoTiempo.Text, out var tiempo);

            var nuevo = new Recurso
            {
                Nombre = nombre,
                CostoUnitario = costo,
                InventarioDisponible = inventario,
                TiempoProduccionMinutos = tiempo
            };

            foreach (var item in lstComponentes.Items)
            {
                if (item is Componente comp)
                    nuevo.Componentes.Add(new Componente { Recurso = comp.Recurso, CantidadNecesaria = comp.CantidadNecesaria });
            }

            controller.AgregarRecurso(nuevo);
            RefrescarVistas();
            lstComponentes.Items.Clear();
        }

        private void OnAgregarComponente(object sender, RoutedEventArgs e)
        {
            if (cmbComponentes.SelectedItem is Recurso r && int.TryParse(txtCantidadComponente.Text, out int cant))
            {
                lstComponentes.Items.Add(new Componente { Recurso = r, CantidadNecesaria = cant });
            }
        }

        private void OnSeleccionRecurso(object sender, SelectionChangedEventArgs e)
        {
            if (lstRecursos.SelectedItem is Recurso r)
            {
                recursoEnEdicion = r;
                txtNuevoNombre.Text = r.Nombre;
                txtNuevoCosto.Text = r.CostoUnitario.ToString();
                txtNuevoInventario.Text = r.InventarioDisponible.ToString();
                txtNuevoTiempo.Text = r.TiempoProduccionMinutos.ToString();
                lstComponentes.Items.Clear();
                foreach (var comp in r.Componentes)
                {
                    lstComponentes.Items.Add(new Componente { Recurso = comp.Recurso, CantidadNecesaria = comp.CantidadNecesaria });
                }
            }
        }

        private void OnGuardarRecurso(object sender, RoutedEventArgs e)
        {
            if (recursoEnEdicion == null)
                return;

            var actualizado = new Recurso
            {
                Nombre = txtNuevoNombre.Text,
                CostoUnitario = decimal.TryParse(txtNuevoCosto.Text, out var c) ? c : 0,
                InventarioDisponible = int.TryParse(txtNuevoInventario.Text, out var i) ? i : 0,
                TiempoProduccionMinutos = int.TryParse(txtNuevoTiempo.Text, out var t) ? t : 0,
                Componentes = new List<Componente>()
            };

            foreach (Componente comp in lstComponentes.Items)
                actualizado.Componentes.Add(comp);

            controller.EditarRecurso(recursoEnEdicion, actualizado);
            recursoEnEdicion = null;
            lstComponentes.Items.Clear();
            RefrescarVistas();
            MessageBox.Show("Recurso editado.");
        }

        private void OnEliminarRecurso(object sender, RoutedEventArgs e)
        {
            if (lstRecursos.SelectedItem is Recurso r)
            {
                controller.EliminarRecurso(r);
                RefrescarVistas();
                MessageBox.Show("Recurso eliminado.");
            }
        }

        private void OnGuardarDatos(object sender, RoutedEventArgs e)
        {
            controller.Guardar("datos.json");
            MessageBox.Show("Datos guardados.");
        }

        private void OnCargarDatos(object sender, RoutedEventArgs e)
        {
            controller.Cargar("datos.json");
            cmbRecursos.ItemsSource = controller.Recursos;
            cmbComponentes.ItemsSource = controller.Recursos;
            lstRecursos.ItemsSource = controller.Recursos;
            MessageBox.Show("Datos cargados.");
        }

        private void RefrescarVistas()
        {
            cmbRecursos.Items.Refresh();
            cmbComponentes.Items.Refresh();
            lstRecursos.Items.Refresh();
        }
    }
}
