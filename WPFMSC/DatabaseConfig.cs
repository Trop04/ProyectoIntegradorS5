using MySql.Data.MySqlClient;
using ProyectoIntegradorS5.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;

namespace ProyectoIntegradorS5.Data
{
    public class DatabaseConfig
    {
        // Lee la cadena de conexión desde App.config
        public static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString
            ?? "server=localhost;user=root;password=1234;database=proyectoint";
    }

    public class ProductoRepository
    {
        private readonly string connectionString;

        public ProductoRepository()
        {
            connectionString = DatabaseConfig.ConnectionString;
        }

        public List<Producto> ObtenerTodos()
        {
            var productos = new List<Producto>();

            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            var query = "SELECT id, nombre, descripcion, precio, stock, ruta_imagen FROM productos WHERE activo = 1";
            using var cmd = new MySqlCommand(query, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                productos.Add(new Producto
                {
                    Id = reader.GetInt32("id"),
                    Nombre = reader.GetString("nombre"),
                    Descripcion = reader.IsDBNull("descripcion") ? "" : reader.GetString("descripcion"),
                    Precio = reader.GetDecimal("precio"),
                    Stock = reader.GetInt32("stock"),
                    RutaImagen = reader.IsDBNull("ruta_imagen") ? "" : reader.GetString("ruta_imagen")
                });
            }

            return productos;
        }

        public int Crear(Producto producto)
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            var query = @"INSERT INTO productos (nombre, descripcion, precio, stock, ruta_imagen, activo, fecha_creacion) 
                         VALUES (@Nombre, @Descripcion, @Precio, @Stock, @RutaImagen, 1, NOW());
                         SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Nombre", producto.Nombre);
            cmd.Parameters.AddWithValue("@Descripcion", producto.Descripcion ?? "");
            cmd.Parameters.AddWithValue("@Precio", producto.Precio);
            cmd.Parameters.AddWithValue("@Stock", producto.Stock);
            cmd.Parameters.AddWithValue("@RutaImagen", producto.RutaImagen ?? "");

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Actualizar(Producto producto)
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            var query = @"UPDATE productos SET 
                         nombre = @Nombre, 
                         descripcion = @Descripcion, 
                         precio = @Precio, 
                         stock = @Stock, 
                         ruta_imagen = @RutaImagen,
                         fecha_actualizacion = NOW()
                         WHERE id = @Id";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Id", producto.Id);
            cmd.Parameters.AddWithValue("@Nombre", producto.Nombre);
            cmd.Parameters.AddWithValue("@Descripcion", producto.Descripcion ?? "");
            cmd.Parameters.AddWithValue("@Precio", producto.Precio);
            cmd.Parameters.AddWithValue("@Stock", producto.Stock);
            cmd.Parameters.AddWithValue("@RutaImagen", producto.RutaImagen ?? "");

            cmd.ExecuteNonQuery();
        }

        public void Eliminar(int id)
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            var query = "UPDATE productos SET activo = 0, fecha_eliminacion = NOW() WHERE id = @Id";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Id", id);

            cmd.ExecuteNonQuery();
        }

        public List<Producto> Buscar(string termino)
        {
            var productos = new List<Producto>();

            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            var query = @"SELECT id, nombre, descripcion, precio, stock, ruta_imagen 
                         FROM productos 
                         WHERE activo = 1 AND (nombre LIKE @Termino OR descripcion LIKE @Termino)";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Termino", $"%{termino}%");
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                productos.Add(new Producto
                {
                    Id = reader.GetInt32("id"),
                    Nombre = reader.GetString("nombre"),
                    Descripcion = reader.IsDBNull("descripcion") ? "" : reader.GetString("descripcion"),
                    Precio = reader.GetDecimal("precio"),
                    Stock = reader.GetInt32("stock"),
                    RutaImagen = reader.IsDBNull("ruta_imagen") ? "" : reader.GetString("ruta_imagen")
                });
            }

            return productos;
        }
    }

    public class ClienteRepository
    {
        private readonly string connectionString;

        public ClienteRepository()
        {
            connectionString = DatabaseConfig.ConnectionString;
        }

        // Adaptado a tu estructura de usuarios - ajusta los campos según tu tabla
        public List<Cliente> ObtenerClientes()
        {
            var clientes = new List<Cliente>();

            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            // Asumiendo que los clientes están en la tabla usuarios con tipo = 'cliente'
            var query = @"SELECT id, nombre_completo, correo, telefono, direccion, ciudad 
                         FROM usuarios 
                         WHERE tipo = 'cliente' AND estado = 'activo'";

            using var cmd = new MySqlCommand(query, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                clientes.Add(new Cliente
                {
                    Id = reader.GetInt32("id"),
                    NombreCompleto = reader.GetString("nombre_completo"),
                    Correo = reader.IsDBNull("correo") ? "" : reader.GetString("correo"),
                    Telefono = reader.IsDBNull("telefono") ? "" : reader.GetString("telefono"),
                    Direccion = reader.IsDBNull("direccion") ? "" : reader.GetString("direccion"),
                    Ciudad = reader.IsDBNull("ciudad") ? "" : reader.GetString("ciudad")
                });
            }

            return clientes;
        }

        public List<Cliente> BuscarClientes(string termino)
        {
            var clientes = new List<Cliente>();

            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            var query = @"SELECT id, nombre_completo, correo, telefono, direccion, ciudad 
                         FROM usuarios 
                         WHERE tipo = 'cliente' AND estado = 'activo' 
                         AND (nombre_completo LIKE @Termino OR correo LIKE @Termino)";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Termino", $"%{termino}%");
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                clientes.Add(new Cliente
                {
                    Id = reader.GetInt32("id"),
                    NombreCompleto = reader.GetString("nombre_completo"),
                    Correo = reader.IsDBNull("correo") ? "" : reader.GetString("correo"),
                    Telefono = reader.IsDBNull("telefono") ? "" : reader.GetString("telefono"),
                    Direccion = reader.IsDBNull("direccion") ? "" : reader.GetString("direccion"),
                    Ciudad = reader.IsDBNull("ciudad") ? "" : reader.GetString("ciudad")
                });
            }

            return clientes;
        }
    }

    public class FacturaRepository
    {
        private readonly string connectionString;

        public FacturaRepository()
        {
            connectionString = DatabaseConfig.ConnectionString;
        }

        public int CrearFactura(Factura factura)
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Insertar factura principal
                var queryFactura = @"INSERT INTO facturas (numero_factura, cliente_id, cliente_nombre, 
                                    cliente_direccion, fecha, subtotal, iva, total, estado, fecha_creacion) 
                                    VALUES (@NumeroFactura, @ClienteId, @ClienteNombre, @ClienteDireccion, 
                                    @Fecha, @Subtotal, @Iva, @Total, 'activa', NOW());
                                    SELECT LAST_INSERT_ID();";

                using var cmdFactura = new MySqlCommand(queryFactura, connection, transaction);
                cmdFactura.Parameters.AddWithValue("@NumeroFactura", factura.NumeroFactura);
                cmdFactura.Parameters.AddWithValue("@ClienteId", factura.ClienteId.HasValue ? (object)factura.ClienteId.Value : DBNull.Value);
                cmdFactura.Parameters.AddWithValue("@ClienteNombre", factura.ClienteNombre);
                cmdFactura.Parameters.AddWithValue("@ClienteDireccion", factura.ClienteDireccion ?? "");
                cmdFactura.Parameters.AddWithValue("@Fecha", factura.Fecha);
                cmdFactura.Parameters.AddWithValue("@Subtotal", factura.Subtotal);
                cmdFactura.Parameters.AddWithValue("@Iva", factura.Iva);
                cmdFactura.Parameters.AddWithValue("@Total", factura.Total);

                int facturaId = Convert.ToInt32(cmdFactura.ExecuteScalar());

                // Insertar items de la factura
                var queryItem = @"INSERT INTO factura_items (factura_id, producto_id, producto_nombre, 
                                 precio_unitario, cantidad, subtotal) 
                                 VALUES (@FacturaId, @ProductoId, @ProductoNombre, @PrecioUnitario, 
                                 @Cantidad, @Subtotal)";

                foreach (var item in factura.Items)
                {
                    using var cmdItem = new MySqlCommand(queryItem, connection, transaction);
                    cmdItem.Parameters.AddWithValue("@FacturaId", facturaId);
                    cmdItem.Parameters.AddWithValue("@ProductoId", item.ProductoId);
                    cmdItem.Parameters.AddWithValue("@ProductoNombre", item.Nombre);
                    cmdItem.Parameters.AddWithValue("@PrecioUnitario", item.PrecioUnitario);
                    cmdItem.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                    cmdItem.Parameters.AddWithValue("@Subtotal", item.Subtotal);

                    cmdItem.ExecuteNonQuery();
                }

                transaction.Commit();
                return facturaId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Factura> ObtenerFacturas()
        {
            var facturas = new List<Factura>();

            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            var query = @"SELECT id, numero_factura, cliente_id, cliente_nombre, cliente_direccion, 
                         fecha, subtotal, iva, total, estado 
                         FROM facturas 
                         WHERE estado = 'activa' 
                         ORDER BY fecha DESC";

            using var cmd = new MySqlCommand(query, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                facturas.Add(new Factura
                {
                    Id = reader.GetInt32("id"),
                    NumeroFactura = reader.GetString("numero_factura"),
                    ClienteId = reader.IsDBNull("cliente_id") ? null : reader.GetInt32("cliente_id"),
                    ClienteNombre = reader.GetString("cliente_nombre"),
                    ClienteDireccion = reader.IsDBNull("cliente_direccion") ? "" : reader.GetString("cliente_direccion"),
                    Fecha = reader.GetDateTime("fecha"),
                    Subtotal = reader.GetDecimal("subtotal"),
                    Iva = reader.GetDecimal("iva"),
                    Total = reader.GetDecimal("total")
                });
            }

            return facturas;
        }

        public string ObtenerSiguienteNumeroFactura()
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            var query = "SELECT COALESCE(MAX(CAST(SUBSTRING(numero_factura, 6) AS UNSIGNED)), 0) + 1 FROM facturas";
            using var cmd = new MySqlCommand(query, connection);

            int siguienteNumero = Convert.ToInt32(cmd.ExecuteScalar());
            return $"FACT-{siguienteNumero:000}";
        }
    }

    // Clase modelo para Cliente
    public class Cliente
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string Ciudad { get; set; }

        public override string ToString()
        {
            return NombreCompleto;
        }
    }

    // Clase modelo para Factura
    public class Factura
    {
        public int Id { get; set; }
        public string NumeroFactura { get; set; }
        public int? ClienteId { get; set; }
        public string ClienteNombre { get; set; }
        public string ClienteDireccion { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public List<ItemFactura> Items { get; set; } = new List<ItemFactura>();
    }
}