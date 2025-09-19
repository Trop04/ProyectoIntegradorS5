using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using ProyectoIntegradorS5.Servicios;

namespace ProyectoIntegradorS5.Views
{

    //AVISO, ESTO SERÁ MUY LARGO, lo voy a documentar todo igual que en SimplexService, PERO TENGO UN PLAN ESPECÍFICO, aunque esté documentado y sepan lo que hacen, NO TOCAR
    // - Jonathan

    public class Producto_O : INotifyPropertyChanged
    {
        private string _nombre = "";
        private double _beneficio = 0;
        private double _recurso1 = 0;
        private double _recurso2 = 0;
        private double _recurso3 = 0;

        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(nameof(Nombre)); }
        }

        public double Beneficio
        {
            get => _beneficio;
            set { _beneficio = value; OnPropertyChanged(nameof(Beneficio)); }
        }

        public double Recurso1
        {
            get => _recurso1;
            set { _recurso1 = value; OnPropertyChanged(nameof(Recurso1)); }
        }

        public double Recurso2
        {
            get => _recurso2;
            set { _recurso2 = value; OnPropertyChanged(nameof(Recurso2)); }
        }

        public double Recurso3
        {
            get => _recurso3;
            set { _recurso3 = value; OnPropertyChanged(nameof(Recurso3)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Recurso_O : INotifyPropertyChanged
    {
        private string _nombre = "";
        private double _disponible = 0;
        private string _unidad = "";

        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(nameof(Nombre)); }
        }

        public double Disponible
        {
            get => _disponible;
            set { _disponible = value; OnPropertyChanged(nameof(Disponible)); }
        }

        public string Unidad
        {
            get => _unidad;
            set { _unidad = value; OnPropertyChanged(nameof(Unidad)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ResultadoProducto
    {
        public string Producto { get; set; } = "";
        public double Cantidad { get; set; }
        public double BeneficioTotal { get; set; }
    }

    public class ResultadoRecurso
    {
        public string Recurso { get; set; } = "";
        public double Usado { get; set; }
        public double Disponible { get; set; }
        public double Sobrante => Disponible - Usado;
    }

    public class HistorialSolucion
    {
        public DateTime Fecha { get; set; }
        public int NumProductos { get; set; }
        public int NumRecursos { get; set; }
        public double ValorOptimo { get; set; }
        public string Estado { get; set; } = "";
        public long Tiempo { get; set; }
        public string Notas { get; set; } = "";
    }

    public class ModeloProgramacionLineal
    {
        public ObservableCollection<Producto_O> Productos { get; set; } = new();
        public ObservableCollection<Recurso_O> Recursos { get; set; } = new();
        public bool EsMaximizacion { get; set; } = true;
        public string NombreProyecto { get; set; } = "Nuevo Proyecto";
    }

    // Clase principal del UserControl o Window
    public partial class TabProduccion : UserControl
    {
        private ObservableCollection<Producto_O> productos;
        private ObservableCollection<Recurso_O> recursos;
        private ObservableCollection<ResultadoProducto> resultadosProductos;
        private ObservableCollection<ResultadoRecurso> resultadosRecursos;
        private ObservableCollection<HistorialSolucion> historial;

        private ModeloProgramacionLineal modeloActual;
        private string archivoActual = "";

        public TabProduccion()
        {
            InitializeComponent();
            InicializarDatos();
            ConfigurarEventos();
        }

        private void InicializarDatos()
        {
            productos = new ObservableCollection<Producto_O>();
            recursos = new ObservableCollection<Recurso_O>();
            resultadosProductos = new ObservableCollection<ResultadoProducto>();
            resultadosRecursos = new ObservableCollection<ResultadoRecurso>();
            historial = new ObservableCollection<HistorialSolucion>();

            dgProductos.ItemsSource = productos;
            dgRecursos.ItemsSource = recursos;
            dgResultadosProductos.ItemsSource = resultadosProductos;
            dgResultadosRecursos.ItemsSource = resultadosRecursos;
            dgHistorial.ItemsSource = historial;

            modeloActual = new ModeloProgramacionLineal();

            // Agregar productos y recursos de ejemplo, ESTO puede ser tocado para ejemplo
            AgregarEjemploMesasSillas();
        }

        // Debug, si surge un error usen esto, lo hice para probar si Simplex servía pero puede tracear otros errores
        private void DebugSimplex()
        {
            var simplexSolver = new ProyectoIntegradorS5.Servicios.SimplexService();

            // Crear un ejemplo simple para probar
            var productosTest = new ObservableCollection<Producto_O>
    {
        new Producto_O { Nombre = "X1", Beneficio = 3, Recurso1 = 1, Recurso2 = 2, Recurso3 = 0 },
        new Producto_O { Nombre = "X2", Beneficio = 2, Recurso1 = 2, Recurso2 = 1, Recurso3 = 0 }
    };

            var recursosTest = new ObservableCollection<Recurso_O>
    {
        new Recurso_O { Nombre = "R1", Disponible = 100, Unidad = "unidades" },
        new Recurso_O { Nombre = "R2", Disponible = 80, Unidad = "unidades" }
    };

            var resultado = simplexSolver.ResolverSimple(productosTest, recursosTest, true);

            MessageBox.Show($"Valor Óptimo: {resultado.ValorObjetivo}\n" +
                           $"Estado: {(resultado.EsOptimo ? "Óptimo" : "No óptimo")}\n" +
                           $"Variables: {string.Join(", ", resultado.Solucion.Select(x => $"{x.Key}={x.Value:F2}"))}",
                           "Debug Simplex");
        }

        private void ConfigurarEventos()
        {
            // Eventos de botones principales
            btnNuevo.Click += BtnNuevo_Click;
            btnAbrir.Click += BtnAbrir_Click;
            btnGuardar.Click += BtnGuardar_Click;
            btnExportar.Click += BtnExportar_Click;
            btnResolver.Click += BtnResolver_Click;

            // Eventos de gestión de productos y recursos
            btnAgregarProducto.Click += BtnAgregarProducto_Click;
            btnEliminarProducto.Click += BtnEliminarProducto_Click;
            btnAgregarRecurso.Click += BtnAgregarRecurso_Click;
            btnEliminarRecurso.Click += BtnEliminarRecurso_Click;

            // Eventos de análisis
            btnAnalisisSensibilidad.Click += BtnAnalisisSensibilidad_Click;
            btnSimularCambios.Click += BtnSimularCambios_Click;
            btnLimpiarHistorial.Click += BtnLimpiarHistorial_Click;

            // Eventos de plantillas
            btnEjemploMesas.Click += (s, e) => AgregarEjemploMesasSillas();
            btnEjemploProduccion.Click += (s, e) => AgregarEjemploProduccion();
            btnEjemploMezclas.Click += (s, e) => AgregarEjemploMezclas();
            btnEjemploTransporte.Click += (s, e) => AgregarEjemploTransporte();
        }

        // Gestión de archivos, esto es lo que crea .lp usando FileSystem
        private void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
                LimpiarModelo();
                archivoActual = "";
        }

        private void BtnAbrir_Click(object sender, RoutedEventArgs e)
        {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Filter = "Archivos de Programación Lineal (*.lp)|*.lp|Todos los archivos (*.*)|*.*",
                    DefaultExt = "lp"
                };

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        CargarModelo(dialog.FileName);
                        archivoActual = dialog.FileName;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al abrir el archivo: {ex.Message}", "Error",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(archivoActual))
            {
                GuardarComo();
            }
            else
            {
                GuardarModelo(archivoActual);
            }
        }

        private void GuardarComo()
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Archivos de Programación Lineal (*.lp)|*.lp|Todos los archivos (*.*)|*.*",
                DefaultExt = "lp"
            };

            if (dialog.ShowDialog() == true)
            {
                GuardarModelo(dialog.FileName);
                archivoActual = dialog.FileName;
            }
        }

        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            ExportarResultados();
        }

        // Gestión de productos y recursos
        private void BtnAgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            productos.Add(new Producto_O
            {
                Nombre = $"Producto {productos.Count + 1}",
                Beneficio = 0,
                Recurso1 = 0,
                Recurso2 = 0,
                Recurso3 = 0
            });
        }

        private void BtnEliminarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (dgProductos.SelectedItem is Producto_O producto)
            {
                productos.Remove(producto);
            }
            else
            {
                MessageBox.Show("Seleccione un producto para eliminar.", "Información",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnAgregarRecurso_Click(object sender, RoutedEventArgs e)
        {
            recursos.Add(new Recurso_O
            {
                Nombre = $"Recurso {recursos.Count + 1}",
                Disponible = 0,
                Unidad = "unidades"
            });
        }

        private void BtnEliminarRecurso_Click(object sender, RoutedEventArgs e)
        {
            if (dgRecursos.SelectedItem is Recurso_O recurso)
            {
                recursos.Remove(recurso);
            }
            else
            {
                MessageBox.Show("Seleccione un recurso para eliminar.", "Información",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Resolución del modelo
        private void BtnResolver_Click(object sender, RoutedEventArgs e)
        {
            if (ValidarModelo())
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    var resultado = ResolverModeloProgramacionLineal();
                    stopwatch.Stop();
                    MostrarResultados(resultado, stopwatch.ElapsedMilliseconds);
                    AgregarAlHistorial(resultado, stopwatch.ElapsedMilliseconds);
                    DibujarGraficos(resultado);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    MessageBox.Show($"Error al resolver el modelo: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    txtValorOptimo.Text = "Error";
                    txtEstado.Text = "Error en cálculo";
                    txtTiempo.Text = $"{stopwatch.ElapsedMilliseconds} ms";
                }
            }
        }

        private bool ValidarModelo()
        {
            if (productos.Count == 0)
            {
                MessageBox.Show("Agregue al menos un producto.", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (recursos.Count == 0)
            {
                MessageBox.Show("Agregue al menos un recurso.", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            foreach (var producto in productos)
            {
                if (string.IsNullOrWhiteSpace(producto.Nombre))
                {
                    MessageBox.Show("Todos los productos deben tener nombre.", "Validación",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            foreach (var recurso in recursos)
            {
                if (string.IsNullOrWhiteSpace(recurso.Nombre) || recurso.Disponible < 0)
                {
                    MessageBox.Show("Todos los recursos deben tener nombre y cantidad disponible válida.",
                                  "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        private ResultadoOptimizacion ResolverModeloProgramacionLineal()
        {
            var simplexSolver = new SimplexService();
            bool esMaximizacion;

            if (rbMaximizar.IsChecked == true) { esMaximizacion = true; }
            else { esMaximizacion = false; }
                try
                {
                    // Usar el método conveniente
                    var resultadoSimplex = simplexSolver.ResolverSimple(productos, recursos, esMaximizacion);

                    // Convertir el resultado del Simplex a clase ResultadoOptimizacion
                    var resultado = ConvertirResultadoSimplex(resultadoSimplex);

                    return resultado;
                }
                catch (Exception ex)
                {
                    // Si hay error con el método simple, intentar con el método manual
                    return ResolverConMetodoManual(esMaximizacion);
                }
        }

        private ResultadoOptimizacion ConvertirResultadoSimplex(ProyectoIntegradorS5.Servicios.SimplexService.ResultadoSimplex resultadoSimplex)
        {
            var resultado = new ResultadoOptimizacion();

            if (resultadoSimplex.EsOptimo)
            {
                resultado.ValorOptimo = Math.Abs(resultadoSimplex.ValorObjetivo); // Asegurar valor positivo, IMPORTANTE
                resultado.Estado = "Óptimo";
                resultado.SolucionProductos = new Dictionary<string, double>(resultadoSimplex.Solucion);
                resultado.UsoRecursos = CalcularUsoRecursos(resultadoSimplex.Solucion);
            }
            else if (resultadoSimplex.EsIlimitado)
            {
                resultado.Estado = "Ilimitado";
                resultado.ValorOptimo = double.PositiveInfinity;
                resultado.SolucionProductos = new Dictionary<string, double>();
                resultado.UsoRecursos = new Dictionary<string, double>();
            }
            else if (resultadoSimplex.EsInfactible)
            {
                resultado.Estado = "Infactible";
                resultado.ValorOptimo = 0;
                resultado.SolucionProductos = new Dictionary<string, double>();
                resultado.UsoRecursos = new Dictionary<string, double>();
            }
            else
            {
                resultado.Estado = $"Error: {resultadoSimplex.MensajeError}";
                resultado.ValorOptimo = 0;
                resultado.SolucionProductos = new Dictionary<string, double>();
                resultado.UsoRecursos = new Dictionary<string, double>();
            }

            return resultado;
        }

        private ResultadoOptimizacion ResolverConMetodoManual(bool esMaximizacion)
        {
            var simplexSolver = new SimplexService();

            if (productos.Count == 0 || recursos.Count == 0)
            {
                return new ResultadoOptimizacion
                {
                    Estado = "Error: No hay productos o recursos definidos",
                    ValorOptimo = 0,
                    SolucionProductos = new Dictionary<string, double>(),
                    UsoRecursos = new Dictionary<string, double>()
                };
            }

            try
            {
                // Preparar datos para el solver manual
                var funcionObjetivo = productos.Select(p => p.Beneficio).ToArray();
                var nombresVariables = productos.Select(p => p.Nombre).ToArray();
                var ladoDerecho = recursos.Select(r => r.Disponible).ToArray();
                var tiposRestriccion = Enumerable.Repeat("<=", recursos.Count).ToArray();

                // Crear matriz de restricciones
                var restricciones = new double[recursos.Count, productos.Count];

                for (int i = 0; i < recursos.Count; i++)
                {
                    for (int j = 0; j < productos.Count; j++)
                    {
                        // Obtener consumo del recurso i por el producto j
                        var consumo = ObtenerConsumoRecurso(productos[j], i);
                        restricciones[i, j] = consumo;
                    }
                }

                // Resolver usando el método completo
                var resultadoSimplex = simplexSolver.Resolver(
                    funcionObjetivo,
                    restricciones,
                    ladoDerecho,
                    tiposRestriccion,
                    nombresVariables,
                    esMaximizacion);

                return ConvertirResultadoSimplex(resultadoSimplex);
            }
            catch (Exception ex)
            {
                return new ResultadoOptimizacion
                {
                    Estado = $"Error en solver manual: {ex.Message}",
                    ValorOptimo = 0,
                    SolucionProductos = new Dictionary<string, double>(),
                    UsoRecursos = new Dictionary<string, double>()
                };
            }
        }

        // Método auxiliar para obtener el consumo de un recurso por un producto
        private double ObtenerConsumoRecurso(Producto_O producto, int indiceRecurso)
        {
            // Ajusta según la estructura real de tu clase Producto
            return indiceRecurso switch
            {
                0 => producto.Recurso1,
                1 => producto.Recurso2,
                2 => producto.Recurso3,
                _ => 0
            };
        }

        // Método para calcular el uso real de recursos basado en la solución
        private Dictionary<string, double> CalcularUsoRecursos(Dictionary<string, double> solucion)
        {
            var usoRecursos = new Dictionary<string, double>();

            // Calcular uso real basado en la solución de variables de decisión
            double usoMadera = 0, usoTiempo = 0, usoMano = 0;

            foreach (var producto in productos)
            {
                if (solucion.ContainsKey(producto.Nombre))
                {
                    double cantidad = solucion[producto.Nombre];
                    usoMadera += cantidad * producto.Recurso1;
                    usoTiempo += cantidad * producto.Recurso2;
                    usoMano += cantidad * producto.Recurso3;
                }
            }

            // Asignar a diccionario por nombre de recurso
            for (int i = 0; i < recursos.Count && i < 3; i++)
            {
                double uso = i switch
                {
                    0 => usoMadera,
                    1 => usoTiempo,
                    2 => usoMano,
                    _ => 0
                };
                usoRecursos[recursos[i].Nombre] = uso;
            }

            return usoRecursos;
        }

        // Método adicional para mostrar información detallada del Simplex, esto es opcional y lo usaba más de debug, y no sé si lo implementaré, lo dejo así porque tengo que hacer el catalogo
        private void MostrarDetallesSimplex(SimplexService.ResultadoSimplex resultado)
        {
            if (resultado.EsOptimo)
            {
                var mensaje = $"Solución óptima encontrada en {resultado.Iteraciones} iteraciones.\n\n";
                mensaje += "Variables de decisión:\n";

                foreach (var variable in resultado.Solucion)
                {
                    mensaje += $"{variable.Key}: {variable.Value:F2}\n";
                }

                if (resultado.VariablesHolgura.Count > 0)
                {
                    mensaje += "\nVariables de holgura:\n";
                    foreach (var holgura in resultado.VariablesHolgura)
                    {
                        mensaje += $"{holgura.Key}: {holgura.Value:F2}\n";
                    }
                }

                MessageBox.Show(mensaje, "Detalles de la Solución",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Método para realizar análisis de sensibilidad, opcional igual, pueden probar meterlo en el final
        private void RealizarAnalisisSensibilidad()
        {
            try
            {
                var simplexSolver = new SimplexService();
                var resultadoSimplex = simplexSolver.ResolverSimple(productos, recursos,
                                                                   rbMaximizar.IsChecked == true);

                if (resultadoSimplex.EsOptimo)
                {
                    var analisis = simplexSolver.AnalizarSensibilidad(resultadoSimplex);

                    var mensaje = "Análisis de Sensibilidad:\n\n";
                    mensaje += "Variables Básicas:\n";
                    foreach (var variable in analisis.VariablesBasicas)
                    {
                        mensaje += $"- {variable}\n";
                    }

                    mensaje += "\nPrecios Sombra:\n";
                    foreach (var precio in analisis.PreciosSombra)
                    {
                        mensaje += $"{precio.Key}: {precio.Value:F4}\n";
                    }

                    MessageBox.Show(mensaje, "Análisis de Sensibilidad",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en análisis de sensibilidad: {ex.Message}",
                               "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Clase ResultadoOptimizacion
        public class ResultadoOptimizacion
        {
            public double ValorOptimo { get; set; }
            public string Estado { get; set; } = "";
            public Dictionary<string, double> SolucionProductos { get; set; } = new();
            public Dictionary<string, double> UsoRecursos { get; set; } = new();
        }

        private void MostrarResultados(ResultadoOptimizacion resultado, long tiempoMs)
        {
            // Actualizar resumen
            txtValorOptimo.Text = $"${resultado.ValorOptimo:F2}";
            txtEstado.Text = resultado.Estado;
            txtTiempo.Text = $"{tiempoMs} ms";

            // Limpiar resultados anteriores
            resultadosProductos.Clear();
            resultadosRecursos.Clear();

            // Llenar resultados de productos
            foreach (var kvp in resultado.SolucionProductos)
            {
                var producto = productos.FirstOrDefault(p => p.Nombre == kvp.Key);
                if (producto != null)
                {
                    resultadosProductos.Add(new ResultadoProducto
                    {
                        Producto = kvp.Key,
                        Cantidad = kvp.Value,
                        BeneficioTotal = kvp.Value * producto.Beneficio
                    });
                }
            }

            // Llenar resultados de recursos
            foreach (var recurso in recursos)
            {
                double usado = resultado.UsoRecursos.ContainsKey(recurso.Nombre)
                             ? resultado.UsoRecursos[recurso.Nombre]
                             : 0;

                resultadosRecursos.Add(new ResultadoRecurso
                {
                    Recurso = recurso.Nombre,
                    Usado = usado,
                    Disponible = recurso.Disponible
                });
            }
        }

        private void AgregarAlHistorial(ResultadoOptimizacion resultado, long tiempoMs)
        {
            historial.Add(new HistorialSolucion
            {
                Fecha = DateTime.Now,
                NumProductos = productos.Count,
                NumRecursos = recursos.Count,
                ValorOptimo = resultado.ValorOptimo,
                Estado = resultado.Estado,
                Tiempo = tiempoMs,
                Notas = $"Objetivo: {(rbMaximizar.IsChecked == true ? "Maximizar" : "Minimizar")}"
            });
        }

        private void DibujarGraficos(ResultadoOptimizacion resultado)
        {
            DibujarGraficoRecursos();

            // Si hay exactamente 2 productos, dibujar región factible
            if (productos.Count == 2 && recursos.Count >= 2)
            {
                DibujarRegionFactible();
            }
        }

        private void DibujarGraficoRecursos()
        {
            canvasGraficoRecursos.Children.Clear();

            if (resultadosRecursos.Count == 0) return;

            double canvasWidth = canvasGraficoRecursos.ActualWidth > 0 ? canvasGraficoRecursos.ActualWidth : 400;
            double canvasHeight = canvasGraficoRecursos.ActualHeight > 0 ? canvasGraficoRecursos.ActualHeight : 300;

            double barWidth = (canvasWidth - 40) / (resultadosRecursos.Count * 2);
            double maxValue = resultadosRecursos.Max(r => r.Disponible);

            if (maxValue == 0) return;

            for (int i = 0; i < resultadosRecursos.Count; i++)
            {
                var recurso = resultadosRecursos[i];
                double x = 20 + i * barWidth * 2;

                // Barra de disponible (fondo)
                double alturaDisponible = (recurso.Disponible / maxValue) * (canvasHeight - 60);
                Rectangle barraDisponible = new Rectangle
                {
                    Width = barWidth,
                    Height = alturaDisponible,
                    Fill = new SolidColorBrush(Color.FromRgb(220, 220, 220))
                };
                Canvas.SetLeft(barraDisponible, x);
                Canvas.SetBottom(barraDisponible, 20);
                canvasGraficoRecursos.Children.Add(barraDisponible);

                // Barra de usado
                double alturaUsado = (recurso.Usado / maxValue) * (canvasHeight - 60);
                Rectangle barraUsado = new Rectangle
                {
                    Width = barWidth,
                    Height = alturaUsado,
                    Fill = new SolidColorBrush(Color.FromRgb(163, 190, 140))
                };
                Canvas.SetLeft(barraUsado, x);
                Canvas.SetBottom(barraUsado, 20);
                canvasGraficoRecursos.Children.Add(barraUsado);

                // Etiqueta del recurso
                TextBlock etiqueta = new TextBlock
                {
                    Text = recurso.Recurso,
                    FontSize = 10,
                    TextAlignment = TextAlignment.Center,
                    Width = barWidth
                };
                Canvas.SetLeft(etiqueta, x);
                Canvas.SetBottom(etiqueta, 5);
                canvasGraficoRecursos.Children.Add(etiqueta);
            }

            // Leyenda
            TextBlock leyendaDisponible = new TextBlock
            {
                Text = "□ Disponible",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100))
            };
            Canvas.SetLeft(leyendaDisponible, 20);
            Canvas.SetTop(leyendaDisponible, 10);
            canvasGraficoRecursos.Children.Add(leyendaDisponible);

            TextBlock leyendaUsado = new TextBlock
            {
                Text = "■ Usado",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(163, 190, 140))
            };
            Canvas.SetLeft(leyendaUsado, 100);
            Canvas.SetTop(leyendaUsado, 10);
            canvasGraficoRecursos.Children.Add(leyendaUsado);
        }

        private void DibujarRegionFactible()
        {
            canvasRegionFactible.Children.Clear();

            if (productos.Count != 2 || recursos.Count < 2) return;

            double canvasWidth = canvasRegionFactible.ActualWidth > 0 ? canvasRegionFactible.ActualWidth : 400;
            double canvasHeight = canvasRegionFactible.ActualHeight > 0 ? canvasRegionFactible.ActualHeight : 300;

            // Dibujar ejes
            Line ejeX = new Line
            {
                X1 = 40,
                Y1 = canvasHeight - 40,
                X2 = canvasWidth - 20,
                Y2 = canvasHeight - 40,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            canvasRegionFactible.Children.Add(ejeX);

            Line ejeY = new Line
            {
                X1 = 40,
                Y1 = 20,
                X2 = 40,
                Y2 = canvasHeight - 40,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            canvasRegionFactible.Children.Add(ejeY);

            // Etiquetas de los ejes
            TextBlock etiquetaX = new TextBlock
            {
                Text = productos[0].Nombre,
                FontSize = 12
            };
            Canvas.SetLeft(etiquetaX, canvasWidth - 100);
            Canvas.SetTop(etiquetaX, canvasHeight - 35);
            canvasRegionFactible.Children.Add(etiquetaX);

            TextBlock etiquetaY = new TextBlock
            {
                Text = productos[1].Nombre,
                FontSize = 12
            };
            Canvas.SetLeft(etiquetaY, 5);
            Canvas.SetTop(etiquetaY, 10);
            canvasRegionFactible.Children.Add(etiquetaY);

            // Aquí se dibujarían las restricciones y la región factible
            // Implementación simplificada para demostración
            Polygon regionFactible = new Polygon
            {
                Fill = new SolidColorBrush(Color.FromArgb(100, 129, 161, 193)),
                Stroke = new SolidColorBrush(Color.FromRgb(129, 161, 193)),
                StrokeThickness = 2
            };

            regionFactible.Points.Add(new Point(40, canvasHeight - 40));
            regionFactible.Points.Add(new Point(200, canvasHeight - 40));
            regionFactible.Points.Add(new Point(200, 100));
            regionFactible.Points.Add(new Point(100, 50));
            regionFactible.Points.Add(new Point(40, 150));

            canvasRegionFactible.Children.Add(regionFactible);

            // Punto óptimo
            if (resultadosProductos.Count >= 2)
            {
                double x = 40 + (resultadosProductos[0].Cantidad / 100) * (canvasWidth - 60);
                double y = canvasHeight - 40 - (resultadosProductos[1].Cantidad / 100) * (canvasHeight - 60);

                Ellipse puntoOptimo = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = Brushes.Red,
                    Stroke = Brushes.DarkRed,
                    StrokeThickness = 2
                };
                Canvas.SetLeft(puntoOptimo, x - 4);
                Canvas.SetTop(puntoOptimo, y - 4);
                canvasRegionFactible.Children.Add(puntoOptimo);

                // Etiqueta del punto óptimo
                TextBlock etiquetaPunto = new TextBlock
                {
                    Text = $"Óptimo ({resultadosProductos[0].Cantidad:F1}, {resultadosProductos[1].Cantidad:F1})",
                    FontSize = 10,
                    Background = Brushes.White,
                    Padding = new Thickness(2)
                };
                Canvas.SetLeft(etiquetaPunto, x + 10);
                Canvas.SetTop(etiquetaPunto, y - 10);
                canvasRegionFactible.Children.Add(etiquetaPunto);
            }
        }

        // Análisis y funciones extra
        private void BtnAnalisisSensibilidad_Click(object sender, RoutedEventArgs e)
        {
            if (resultadosProductos.Count == 0)
            {
                MessageBox.Show("Primero debe resolver un modelo.", "Información",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Implementar análisis de sensibilidad
            string analisis = GenerarAnalisisSensibilidad();

            MessageBox.Show(analisis, "Análisis de Sensibilidad",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string GenerarAnalisisSensibilidad()
        {
            var analisis = "ANÁLISIS DE SENSIBILIDAD\n";
            analisis += new string('=', 50) + "\n\n";

            analisis += "Recursos Críticos (Completamente utilizados):\n";
            foreach (var recurso in resultadosRecursos.Where(r => Math.Abs(r.Sobrante) < 0.01))
            {
                analisis += $"- {recurso.Recurso}: {recurso.Usado:F2}/{recurso.Disponible:F2}\n";
            }

            analisis += "\nRecursos con Holgura:\n";
            foreach (var recurso in resultadosRecursos.Where(r => r.Sobrante > 0.01))
            {
                analisis += $"- {recurso.Recurso}: Sobrante {recurso.Sobrante:F2}\n";
            }

            analisis += "\nProductos en la Solución Óptima:\n";
            foreach (var producto in resultadosProductos.Where(p => p.Cantidad > 0.01))
            {
                analisis += $"- {producto.Producto}: {producto.Cantidad:F2} unidades\n";
            }

            return analisis;
        }

        private void BtnSimularCambios_Click(object sender, RoutedEventArgs e)
        {
            if (recursos.Count == 0)
            {
                MessageBox.Show("No hay recursos para simular cambios.", "Información",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Crear ventana de simulación (implementación simplificada)
            string simulacion = "SIMULACIÓN DE CAMBIOS\n";
            simulacion += new string('=', 40) + "\n\n";

            foreach (var recurso in recursos.Take(3))
            {
                double incremento10 = recurso.Disponible * 1.1;
                double decremento10 = recurso.Disponible * 0.9;

                simulacion += $"{recurso.Nombre}:\n";
                simulacion += $"  Actual: {recurso.Disponible:F2}\n";
                simulacion += $"  +10%: {incremento10:F2} (Impacto estimado: +5-15%)\n";
                simulacion += $"  -10%: {decremento10:F2} (Impacto estimado: -5-15%)\n\n";
            }

            MessageBox.Show(simulacion, "Simulación de Cambios",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLimpiarHistorial_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Está seguro de que desea limpiar el historial?",
                              "Confirmar", MessageBoxButton.YesNo,
                              MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                historial.Clear();
            }
        }

        // Gestión de archivos
        private void LimpiarModelo()
        {
            productos.Clear();
            recursos.Clear();
            resultadosProductos.Clear();
            resultadosRecursos.Clear();

            txtValorOptimo.Text = "$0.00";
            txtEstado.Text = "Sin resolver";
            txtTiempo.Text = "0 ms";

            canvasGraficoRecursos.Children.Clear();
            canvasRegionFactible.Children.Clear();
        }

        private void CargarModelo(string archivo)
        {
            string json = File.ReadAllText(archivo);
            modeloActual = JsonSerializer.Deserialize<ModeloProgramacionLineal>(json)
                          ?? new ModeloProgramacionLineal();

            productos.Clear();
            recursos.Clear();

            foreach (var producto in modeloActual.Productos)
                productos.Add(producto);

            foreach (var recurso in modeloActual.Recursos)
                recursos.Add(recurso);

            rbMaximizar.IsChecked = modeloActual.EsMaximizacion;
            rbMinimizar.IsChecked = !modeloActual.EsMaximizacion;
        }

        private void GuardarModelo(string archivo)
        {
            modeloActual.Productos.Clear();
            modeloActual.Recursos.Clear();

            foreach (var producto in productos)
                modeloActual.Productos.Add(producto);

            foreach (var recurso in recursos)
                modeloActual.Recursos.Add(recurso);

            modeloActual.EsMaximizacion = rbMaximizar.IsChecked == true;

            string json = JsonSerializer.Serialize(modeloActual, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(archivo, json);

            MessageBox.Show("Modelo guardado exitosamente.", "Información",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportarResultados()
        {
            if (resultadosProductos.Count == 0)
            {
                MessageBox.Show("No hay resultados para exportar. Primero resuelva el modelo.",
                              "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Archivo de texto (*.txt)|*.txt|CSV (*.csv)|*.csv|Todos los archivos (*.*)|*.*",
                DefaultExt = "txt"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    GenerarReporte(dialog.FileName);
                    MessageBox.Show("Reporte exportado exitosamente.", "Información",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void GenerarReporte(string archivo)
        {
            using (StreamWriter writer = new StreamWriter(archivo))
            {
                writer.WriteLine("REPORTE DE OPTIMIZACIÓN DE PRODUCCIÓN");
                writer.WriteLine(new string('=', 50));
                writer.WriteLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                writer.WriteLine($"Objetivo: {(rbMaximizar.IsChecked == true ? "Maximizar Beneficio" : "Minimizar Costo")}");
                writer.WriteLine($"Valor Óptimo: {txtValorOptimo.Text}");
                writer.WriteLine($"Estado: {txtEstado.Text}");
                writer.WriteLine($"Tiempo de cálculo: {txtTiempo.Text}");
                writer.WriteLine();

                writer.WriteLine("PRODUCCIÓN ÓPTIMA:");
                writer.WriteLine(new string('-', 30));
                foreach (var producto in resultadosProductos)
                {
                    writer.WriteLine($"{producto.Producto}: {producto.Cantidad:F2} unidades " +
                                   $"(Beneficio: ${producto.BeneficioTotal:F2})");
                }
                writer.WriteLine();

                writer.WriteLine("USO DE RECURSOS:");
                writer.WriteLine(new string('-', 30));
                foreach (var recurso in resultadosRecursos)
                {
                    writer.WriteLine($"{recurso.Recurso}: {recurso.Usado:F2}/{recurso.Disponible:F2} " +
                                   $"(Sobrante: {recurso.Sobrante:F2})");
                }
                writer.WriteLine();

                if (chkIncluirHistorial.IsChecked == true && historial.Count > 0)
                {
                    writer.WriteLine("HISTORIAL DE SOLUCIONES:");
                    writer.WriteLine(new string('-', 30));
                    foreach (var solucion in historial.TakeLast(10))
                    {
                        writer.WriteLine($"{solucion.Fecha:dd/MM HH:mm} - " +
                                       $"Valor: ${solucion.ValorOptimo:F2} - " +
                                       $"Estado: {solucion.Estado} - " +
                                       $"Tiempo: {solucion.Tiempo}ms");
                    }
                }
            }
        }

        // Plantillas de ejemplo
        private void AgregarEjemploMesasSillas()
        {
            LimpiarModelo();

            productos.Add(new Producto_O
            {
                Nombre = "Mesas",
                Beneficio = 300,
                Recurso1 = 4, // Madera
                Recurso2 = 3, // Tiempo
                Recurso3 = 2  // Mano de obra
            });

            productos.Add(new Producto_O
            {
                Nombre = "Sillas",
                Beneficio = 150,
                Recurso1 = 2, // Madera
                Recurso2 = 1, // Tiempo
                Recurso3 = 1  // Mano de obra
            });

            recursos.Add(new Recurso_O
            {
                Nombre = "Madera",
                Disponible = 1000,
                Unidad = "m²"
            });

            recursos.Add(new Recurso_O
            {
                Nombre = "Tiempo Máquina",
                Disponible = 600,
                Unidad = "horas"
            });

            recursos.Add(new Recurso_O
            {
                Nombre = "Mano de Obra",
                Disponible = 400,
                Unidad = "horas"
            });
        }

        private void AgregarEjemploProduccion()
        {
            LimpiarModelo();

            productos.Add(new Producto_O { Nombre = "Producto A", Beneficio = 50, Recurso1 = 2, Recurso2 = 4, Recurso3 = 1 });
            productos.Add(new Producto_O { Nombre = "Producto B", Beneficio = 40, Recurso1 = 3, Recurso2 = 2, Recurso3 = 2 });
            productos.Add(new Producto_O { Nombre = "Producto C", Beneficio = 60, Recurso1 = 1, Recurso2 = 3, Recurso3 = 3 });

            recursos.Add(new Recurso_O { Nombre = "Material Principal", Disponible = 500, Unidad = "kg" });
            recursos.Add(new Recurso_O { Nombre = "Tiempo Proceso", Disponible = 800, Unidad = "horas" });
            recursos.Add(new Recurso_O { Nombre = "Capacidad Línea", Disponible = 300, Unidad = "unidades" });
        }

        private void AgregarEjemploMezclas()
        {
            LimpiarModelo();
            rbMinimizar.IsChecked = true;

            productos.Add(new Producto_O { Nombre = "Ingrediente X", Beneficio = 2.5, Recurso1 = 1, Recurso2 = 0.5, Recurso3 = 0.2 });
            productos.Add(new Producto_O { Nombre = "Ingrediente Y", Beneficio = 1.8, Recurso1 = 0.8, Recurso2 = 1, Recurso3 = 0.3 });

            recursos.Add(new Recurso_O { Nombre = "Proteína Mínima", Disponible = 20, Unidad = "g" });
            recursos.Add(new Recurso_O { Nombre = "Vitaminas Mínimas", Disponible = 15, Unidad = "UI" });
            recursos.Add(new Recurso_O { Nombre = "Fibra Mínima", Disponible = 8, Unidad = "g" });
        }

        private void AgregarEjemploTransporte()
        {
            LimpiarModelo();
            rbMinimizar.IsChecked = true;

            productos.Add(new Producto_O { Nombre = "Ruta A->1", Beneficio = 8, Recurso1 = 1, Recurso2 = 0, Recurso3 = 1 });
            productos.Add(new Producto_O { Nombre = "Ruta A->2", Beneficio = 12, Recurso1 = 1, Recurso2 = 1, Recurso3 = 0 });
            productos.Add(new Producto_O { Nombre = "Ruta B->1", Beneficio = 6, Recurso1 = 0, Recurso2 = 1, Recurso3 = 1 });
            productos.Add(new Producto_O { Nombre = "Ruta B->2", Beneficio = 10, Recurso1 = 0, Recurso2 = 1, Recurso3 = 0 });

            recursos.Add(new Recurso_O { Nombre = "Suministro A", Disponible = 100, Unidad = "unidades" });
            recursos.Add(new Recurso_O { Nombre = "Suministro B", Disponible = 150, Unidad = "unidades" });
            recursos.Add(new Recurso_O { Nombre = "Demanda Destino 1", Disponible = 80, Unidad = "unidades" });
        }
    }

    // Clase para resultados de optimización
    public class ResultadoOptimizacion
    {
        public double ValorOptimo { get; set; }
        public string Estado { get; set; } = "";
        public Dictionary<string, double> SolucionProductos { get; set; } = new();
        public Dictionary<string, double> UsoRecursos { get; set; } = new();
    }
}

