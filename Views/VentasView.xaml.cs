using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Documents;
using System.Windows.Xps;
using ProyectoIntegradorS5.Data;
using Microsoft.Win32;

namespace ProyectoIntegradorS5.Views
{
    public partial class VentasView : UserControl
    {

        private readonly ProductoRepository productoRepository;
        private readonly ClienteRepository clienteRepository;
        private readonly FacturaRepository facturaRepository;


        private ObservableCollection<Producto> productos;
        private ObservableCollection<ItemFactura> itemsFactura;
        private ObservableCollection<Cliente> clientes;
        private List<Producto> productosOriginales; 
        private Producto productoEditando;
        private Cliente clienteSeleccionado;
        private string numeroFacturaActual;

        public VentasView()
        {
            InitializeComponent();

            productoRepository = new ProductoRepository();
            clienteRepository = new ClienteRepository();
            facturaRepository = new FacturaRepository();

            InicializarDatos();
        }

        private void InicializarDatos()
        {
            try
            {

                productos = new ObservableCollection<Producto>();
                productosOriginales = new List<Producto>();
                itemsFactura = new ObservableCollection<ItemFactura>();
                clientes = new ObservableCollection<Cliente>();


                ProductosDataGrid.ItemsSource = productos;
                FacturaDataGrid.ItemsSource = itemsFactura;
                ProductoComboBox.ItemsSource = productos;
                ClienteComboBox.ItemsSource = clientes;


                FechaDatePicker.SelectedDate = DateTime.Now;

                CargarProductos();
                CargarClientes();
                ActualizarNumeroFactura();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar datos: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarProductos()
        {
            try
            {
                var productosDB = productoRepository.ObtenerTodos();

                productos.Clear();
                productosOriginales.Clear();

                foreach (var producto in productosDB)
                {
                    productos.Add(producto);
                    productosOriginales.Add(producto);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarClientes()
        {
            try
            {
                var clientesDB = clienteRepository.ObtenerClientes();

                clientes.Clear();
                foreach (var cliente in clientesDB)
                {
                    clientes.Add(cliente);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Eventos del Catálogo

        private void SeleccionarImagen_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Title = "Seleccionar Imagen",
                Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Todos los archivos|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                RutaImagenTextBox.Text = openFileDialog.FileName;
                MostrarVistaPrevia(openFileDialog.FileName);
            }
        }

        private void MostrarVistaPrevia(string rutaImagen)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(rutaImagen);
                bitmap.DecodePixelWidth = 120;
                bitmap.EndInit();
                ImagenPreview.Source = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la imagen: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GuardarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCamposProducto())
                return;

            try
            {
                if (productoEditando == null)
                {
                    // Nuevo producto
                    var nuevoProducto = new Producto
                    {
                        Nombre = NombreTextBox.Text.Trim(),
                        Descripcion = DescripcionTextBox.Text.Trim(),
                        Precio = decimal.Parse(PrecioTextBox.Text),
                        Stock = int.Parse(StockTextBox.Text),
                        RutaImagen = RutaImagenTextBox.Text.Trim()
                    };

                    // Guardar en base de datos
                    int nuevoId = productoRepository.Crear(nuevoProducto);
                    nuevoProducto.Id = nuevoId;

                    // Actualizar colecciones locales
                    productos.Add(nuevoProducto);
                    productosOriginales.Add(nuevoProducto);

                    MessageBox.Show("Producto agregado exitosamente.", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Editar producto existente
                    productoEditando.Nombre = NombreTextBox.Text.Trim();
                    productoEditando.Descripcion = DescripcionTextBox.Text.Trim();
                    productoEditando.Precio = decimal.Parse(PrecioTextBox.Text);
                    productoEditando.Stock = int.Parse(StockTextBox.Text);
                    productoEditando.RutaImagen = RutaImagenTextBox.Text.Trim();

                    // Actualizar en base de datos
                    productoRepository.Actualizar(productoEditando);

                    // Actualizar el producto en la lista original
                    var productoOriginal = productosOriginales.FirstOrDefault(p => p.Id == productoEditando.Id);
                    if (productoOriginal != null)
                    {
                        productoOriginal.Nombre = productoEditando.Nombre;
                        productoOriginal.Descripcion = productoEditando.Descripcion;
                        productoOriginal.Precio = productoEditando.Precio;
                        productoOriginal.Stock = productoEditando.Stock;
                        productoOriginal.RutaImagen = productoEditando.RutaImagen;
                    }

                    ProductosDataGrid.Items.Refresh();
                    MessageBox.Show("Producto actualizado exitosamente.", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LimpiarCamposProducto();
                productoEditando = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el producto: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarCamposProducto()
        {
            if (string.IsNullOrWhiteSpace(NombreTextBox.Text))
            {
                MessageBox.Show("El nombre del producto es obligatorio.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(PrecioTextBox.Text, out decimal precio) || precio <= 0)
            {
                MessageBox.Show("El precio debe ser un número válido mayor que cero.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(StockTextBox.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("El stock debe ser un número válido mayor o igual a cero.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void NuevoProducto_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCamposProducto();
            productoEditando = null;
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCamposProducto();
            productoEditando = null;
        }

        private void LimpiarCamposProducto()
        {
            NombreTextBox.Clear();
            DescripcionTextBox.Clear();
            PrecioTextBox.Clear();
            StockTextBox.Clear();
            RutaImagenTextBox.Clear();
            ImagenPreview.Source = null;
        }

        private void EditarProducto_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var producto = button?.Tag as Producto;

            if (producto != null)
            {
                productoEditando = producto;
                NombreTextBox.Text = producto.Nombre;
                DescripcionTextBox.Text = producto.Descripcion;
                PrecioTextBox.Text = producto.Precio.ToString();
                StockTextBox.Text = producto.Stock.ToString();
                RutaImagenTextBox.Text = producto.RutaImagen ?? "";

                if (!string.IsNullOrEmpty(producto.RutaImagen) && File.Exists(producto.RutaImagen))
                {
                    MostrarVistaPrevia(producto.RutaImagen);
                }
            }
        }

        private void EliminarProducto_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var producto = button?.Tag as Producto;

            if (producto != null)
            {
                var resultado = MessageBox.Show($"¿Está seguro de eliminar el producto '{producto.Nombre}'?",
                    "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Eliminar de la base de datos (soft delete)
                        productoRepository.Eliminar(producto.Id);

                        // Actualizar colecciones locales
                        productos.Remove(producto);
                        productosOriginales.Remove(producto);

                        MessageBox.Show("Producto eliminado exitosamente.", "Éxito",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar el producto: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BuscarTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textoBusqueda = BuscarTextBox.Text.ToLower().Trim();

            try
            {
                if (string.IsNullOrEmpty(textoBusqueda))
                {
                    ProductosDataGrid.ItemsSource = productosOriginales;
                }
                else
                {
                    // Buscar en base de datos para resultados más precisos, Allan checa luego la BD para probar esto
                    var productosFiltrados = productoRepository.Buscar(textoBusqueda);
                    ProductosDataGrid.ItemsSource = productosFiltrados;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en la búsqueda: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Eventos de Facturación

        private void ClienteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            clienteSeleccionado = ClienteComboBox.SelectedItem as Cliente;

            if (clienteSeleccionado != null)
            {
                DireccionTextBox.Text = $"{clienteSeleccionado.Direccion}, {clienteSeleccionado.Ciudad}".Trim().TrimEnd(',');
            }
            else
            {
                DireccionTextBox.Text = "";
            }
        }

        private void BuscarCliente_Click(object sender, RoutedEventArgs e)
        {
            var textoBusqueda = ClienteComboBox.Text.Trim();

            if (string.IsNullOrEmpty(textoBusqueda))
            {
                CargarClientes();
                return;
            }

            try
            {
                var clientesFiltrados = clienteRepository.BuscarClientes(textoBusqueda);

                clientes.Clear();
                foreach (var cliente in clientesFiltrados)
                {
                    clientes.Add(cliente);
                }

                if (clientes.Count == 0)
                {
                    MessageBox.Show("No se encontraron clientes con ese criterio.", "Búsqueda",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar clientes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AgregarAFactura_Click(object sender, RoutedEventArgs e)
        {
            var productoSeleccionado = ProductoComboBox.SelectedItem as Producto;

            if (productoSeleccionado == null)
            {
                MessageBox.Show("Por favor seleccione un producto.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(CantidadTextBox.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("La cantidad debe ser un número válido mayor que cero.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cantidad > productoSeleccionado.Stock)
            {
                MessageBox.Show($"No hay suficiente stock. Stock disponible: {productoSeleccionado.Stock}",
                    "Stock Insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Verifica si el producto ya está en la factura
            var itemExistente = itemsFactura.FirstOrDefault(i => i.ProductoId == productoSeleccionado.Id);

            if (itemExistente != null)
            {
                itemExistente.Cantidad += cantidad;
                itemExistente.Subtotal = itemExistente.Cantidad * itemExistente.PrecioUnitario;
            }
            else
            {
                var nuevoItem = new ItemFactura
                {
                    ProductoId = productoSeleccionado.Id,
                    Nombre = productoSeleccionado.Nombre,
                    PrecioUnitario = productoSeleccionado.Precio,
                    Cantidad = cantidad,
                    Subtotal = productoSeleccionado.Precio * cantidad
                };

                itemsFactura.Add(nuevoItem);
            }

            ActualizarTotales();
            CantidadTextBox.Text = "1";
            ProductoComboBox.SelectedIndex = -1;
        }

        private void EliminarDeFactura_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as ItemFactura;

            if (item != null)
            {
                itemsFactura.Remove(item);
                ActualizarTotales();
            }
        }

        private void ActualizarTotales()
        {
            var subtotal = itemsFactura.Sum(i => i.Subtotal);
            var iva = subtotal * 0.15m;
            var total = subtotal + iva;

            SubtotalLabel.Text = subtotal.ToString("C");
            IVALabel.Text = iva.ToString("C");
            TotalLabel.Text = total.ToString("C");

        }

        private void NuevaFactura_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFactura();
            ActualizarNumeroFactura();
        }

        private void LimpiarFactura()
        {
            itemsFactura.Clear();
            ClienteComboBox.SelectedIndex = -1;
            ClienteComboBox.Text = "";
            DireccionTextBox.Clear();
            FechaDatePicker.SelectedDate = DateTime.Now;
            clienteSeleccionado = null;
            ActualizarTotales();
        }

        private void ActualizarNumeroFactura()
        {
            try
            {
                numeroFacturaActual = facturaRepository.ObtenerSiguienteNumeroFactura();
                NumeroFacturaLabel.Text = numeroFacturaActual;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener número de factura: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                numeroFacturaActual = "FACT-001";
                NumeroFacturaLabel.Text = numeroFacturaActual;
            }
        }

        private void GuardarFactura_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFactura())
                return;

            try
            {
                // Crear objeto factura
                var factura = new Factura
                {
                    NumeroFactura = numeroFacturaActual,
                    ClienteId = clienteSeleccionado?.Id,
                    ClienteNombre = clienteSeleccionado?.NombreCompleto ?? ClienteComboBox.Text.Trim(),
                    ClienteDireccion = DireccionTextBox.Text.Trim(),
                    Fecha = FechaDatePicker.SelectedDate.Value,
                    Subtotal = itemsFactura.Sum(i => i.Subtotal),
                    Iva = itemsFactura.Sum(i => i.Subtotal) * 0.15m,
                    Total = itemsFactura.Sum(i => i.Subtotal) * 1.15m,
                    Items = itemsFactura.ToList()
                };

                // Guardar en base de datos
                int facturaId = facturaRepository.CrearFactura(factura);

                MessageBox.Show($"Factura {numeroFacturaActual} guardada exitosamente con ID: {facturaId}", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                LimpiarFactura();
                ActualizarNumeroFactura();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la factura: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarFactura()
        {
            if (clienteSeleccionado == null && string.IsNullOrWhiteSpace(ClienteComboBox.Text))
            {
                MessageBox.Show("El nombre del cliente es obligatorio.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (itemsFactura.Count == 0)
            {
                MessageBox.Show("Debe agregar al menos un producto a la factura.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!FechaDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("La fecha es obligatoria.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ImprimirFactura_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFactura())
                return;

            try
            {
                // Crear documento para imprimir
                var documento = CrearDocumentoFactura();

                // Mostrar vista previa de impresión
                var ventanaPreview = new Window()
                {
                    Title = "Vista Previa de Factura",
                    Width = 800,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                var documentViewer = new DocumentViewer()
                {
                    Document = documento
                };

                ventanaPreview.Content = documentViewer;
                ventanaPreview.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar la factura: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FixedDocument CrearDocumentoFactura()
        {
            var documento = new FixedDocument();
            var pagina = new FixedPage()
            {
                Width = 793.7, // A4 width
                Height = 1122.5 // A4 height
            };

            // Crear el contenido de la factura
            var grid = new Grid()
            {
                Margin = new Thickness(50)
            };

            // Definir filas del grid
            for (int i = 0; i < 8; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = i == 4 ? new GridLength(1, GridUnitType.Star) : GridLength.Auto });
            }

            // Encabezado de la empresa
            var tituloEmpresa = new TextBlock()
            {
                Text = "MUEBLES LÓPEZ S.A",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            Grid.SetRow(tituloEmpresa, 0);
            grid.Children.Add(tituloEmpresa);

            // Información de la factura
            var infoFactura = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var infoIzquierda = new StackPanel();
            infoIzquierda.Children.Add(new TextBlock() { Text = $"Factura: {NumeroFacturaLabel.Text}", FontWeight = FontWeights.Bold, FontSize = 16 });
            infoIzquierda.Children.Add(new TextBlock() { Text = $"Fecha: {FechaDatePicker.SelectedDate?.ToString("dd/MM/yyyy")}", FontSize = 14 });

            var infoDerecha = new StackPanel();
            infoDerecha.Children.Add(new TextBlock() { Text = "RFC: 123456789", FontSize = 12 });
            infoDerecha.Children.Add(new TextBlock() { Text = "Tel: (593) 123-4567", FontSize = 12 });

            infoFactura.Children.Add(infoIzquierda);
            infoFactura.Children.Add(infoDerecha);
            Grid.SetRow(infoFactura, 1);
            grid.Children.Add(infoFactura);

            // Información del cliente
            var infoCliente = new StackPanel()
            {
                Margin = new Thickness(0, 0, 0, 20)
            };
            infoCliente.Children.Add(new TextBlock() { Text = "FACTURAR A:", FontWeight = FontWeights.Bold, FontSize = 14 });
            var nombreCliente = clienteSeleccionado?.NombreCompleto ?? ClienteComboBox.Text;
            infoCliente.Children.Add(new TextBlock() { Text = nombreCliente, FontSize = 12, Margin = new Thickness(0, 5, 0, 0) });
            if (!string.IsNullOrWhiteSpace(DireccionTextBox.Text))
            {
                infoCliente.Children.Add(new TextBlock() { Text = DireccionTextBox.Text, FontSize = 12 });
            }
            Grid.SetRow(infoCliente, 2);
            grid.Children.Add(infoCliente);

            // Separador
            var separador = new System.Windows.Shapes.Rectangle()
            {
                Height = 2,
                Fill = System.Windows.Media.Brushes.Black,
                Margin = new Thickness(0, 10, 0, 10)
            };
            Grid.SetRow(separador, 3);
            grid.Children.Add(separador);

            // Tabla de productos
            var tablaProductos = CrearTablaProductos();
            Grid.SetRow(tablaProductos, 4);
            grid.Children.Add(tablaProductos);

            // Totales
            var panelTotales = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var subtotal = itemsFactura.Sum(i => i.Subtotal);
            var iva = subtotal * 0.15m;
            var total = subtotal + iva;

            panelTotales.Children.Add(new TextBlock() { Text = $"Subtotal: {subtotal:C}", FontSize = 14, TextAlignment = TextAlignment.Right });
            panelTotales.Children.Add(new TextBlock() { Text = $"IVA (15%): {iva:C}", FontSize = 14, TextAlignment = TextAlignment.Right });
            panelTotales.Children.Add(new System.Windows.Shapes.Rectangle() { Height = 1, Fill = System.Windows.Media.Brushes.Black, Margin = new Thickness(0, 5, 0, 5) });
            panelTotales.Children.Add(new TextBlock() { Text = $"TOTAL: {total:C}", FontSize = 16, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right });

            Grid.SetRow(panelTotales, 5);
            grid.Children.Add(panelTotales);

            // Pie de página
            var piePagina = new TextBlock()
            {
                Text = "¡Gracias por su compra!",
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 14,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 30, 0, 0)
            };
            Grid.SetRow(piePagina, 6);
            grid.Children.Add(piePagina);

            pagina.Children.Add(grid);

            var pageContent = new PageContent();
            pageContent.Child = pagina;
            documento.Pages.Add(pageContent);

            return documento;
        }

        private Grid CrearTablaProductos()
        {
            var tabla = new Grid();

            // Definir columnas
            tabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(3, GridUnitType.Star) }); // Producto
            tabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }); // Precio
            tabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }); // Cantidad
            tabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }); // Subtotal

            // Encabezados
            var headers = new[] { "Producto", "Precio Unit.", "Cantidad", "Subtotal" };
            for (int i = 0; i < headers.Length; i++)
            {
                tabla.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                var headerBorder = new Border()
                {
                    Background = System.Windows.Media.Brushes.LightGray,
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(5)
                };

                var headerText = new TextBlock()
                {
                    Text = headers[i],
                    FontWeight = FontWeights.Bold,
                    FontSize = 12,
                    TextAlignment = TextAlignment.Center
                };

                headerBorder.Child = headerText;
                Grid.SetRow(headerBorder, 0);
                Grid.SetColumn(headerBorder, i);
                tabla.Children.Add(headerBorder);
            }

            // Filas de productos
            for (int fila = 0; fila < itemsFactura.Count; fila++)
            {
                tabla.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                var item = itemsFactura[fila];

                var valores = new[]
                {
                    item.Nombre,
                    item.PrecioUnitario.ToString("C"),
                    item.Cantidad.ToString(),
                    item.Subtotal.ToString("C")
                };

                for (int col = 0; col < valores.Length; col++)
                {
                    var cellBorder = new Border()
                    {
                        BorderBrush = System.Windows.Media.Brushes.Black,
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(5)
                    };

                    var cellText = new TextBlock()
                    {
                        Text = valores[col],
                        FontSize = 11,
                        TextAlignment = col == 0 ? TextAlignment.Left : TextAlignment.Right
                    };

                    cellBorder.Child = cellText;
                    Grid.SetRow(cellBorder, fila + 1);
                    Grid.SetColumn(cellBorder, col);
                    tabla.Children.Add(cellBorder);
                }
            }

            return tabla;
        }

        #endregion
    }

    #region Clases de Modelo

    public class Producto : INotifyPropertyChanged
    {
        private int _id;
        private string _nombre;
        private string _descripcion;
        private decimal _precio;
        private int _stock;
        private string _rutaImagen;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(nameof(Nombre)); }
        }

        public string Descripcion
        {
            get => _descripcion;
            set { _descripcion = value; OnPropertyChanged(nameof(Descripcion)); }
        }

        public decimal Precio
        {
            get => _precio;
            set { _precio = value; OnPropertyChanged(nameof(Precio)); }
        }

        public int Stock
        {
            get => _stock;
            set { _stock = value; OnPropertyChanged(nameof(Stock)); }
        }

        public string RutaImagen
        {
            get => _rutaImagen;
            set { _rutaImagen = value; OnPropertyChanged(nameof(RutaImagen)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ItemFactura : INotifyPropertyChanged
    {
        private int _productoId;
        private string _nombre;
        private decimal _precioUnitario;
        private int _cantidad;
        private decimal _subtotal;

        public int ProductoId
        {
            get => _productoId;
            set { _productoId = value; OnPropertyChanged(nameof(ProductoId)); }
        }

        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(nameof(Nombre)); }
        }

        public decimal PrecioUnitario
        {
            get => _precioUnitario;
            set { _precioUnitario = value; OnPropertyChanged(nameof(PrecioUnitario)); }
        }

        public int Cantidad
        {
            get => _cantidad;
            set { _cantidad = value; OnPropertyChanged(nameof(Cantidad)); }
        }

        public decimal Subtotal
        {
            get => _subtotal;
            set { _subtotal = value; OnPropertyChanged(nameof(Subtotal)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion
}