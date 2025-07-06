using MySql.Data.MySqlClient;
using ProyectoIntegradorS5.Modelos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ProyectoIntegradorS5.Servicios
{
    public class UsuarioService
    {
        private const string connectionString = "server=localhost;user=root;password=1234;database=proyectoint";

        public async Task<List<Usuario>> ObtenerTodosAsync()
        {
            var lista = new List<Usuario>();

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var query = "SELECT * FROM usuarios";
            using var cmd = new MySqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lista.Add(new Usuario
                {
                    Id = reader.GetInt32("id"),
                    NombreCompleto = reader.GetString("nombre_completo"),
                    Correo = reader.GetString("correo"),
                    Telefono = reader.GetString("telefono"),
                    Tipo = reader.GetString("tipo"),
                    Estado = reader.GetString("estado"),
                    FechaCreacion = reader.GetDateTime("fecha_creacion"),

                    // Empleado
                    Cargo = reader.IsDBNull("cargo") ? null : reader.GetString("cargo"),
                    Departamento = reader.IsDBNull("departamento") ? null : reader.GetString("departamento"),
                    Salario = reader.IsDBNull("salario") ? 0 : reader.GetDecimal("salario"),
                    Horario = reader.IsDBNull("horario") ? null : reader.GetString("horario"),

                    // Cliente
                    Direccion = reader.IsDBNull("direccion") ? null : reader.GetString("direccion"),
                    Ciudad = reader.IsDBNull("ciudad") ? null : reader.GetString("ciudad"),
                });
            }

            return lista;
        }

        public void Agregar(Usuario usuario)
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            var query = @"
                INSERT INTO usuarios (nombre_completo, correo, telefono, tipo, estado, fecha_creacion, 
                                      cargo, departamento, salario, horario, direccion, ciudad)
                VALUES (@NombreCompleto, @Correo, @Telefono, @Tipo, @Estado, @FechaCreacion, 
                        @Cargo, @Departamento, @Salario, @Horario, @Direccion, @Ciudad)";

            using var cmd = new MySqlCommand(query, connection);
            AgregarParametros(cmd, usuario);
            cmd.ExecuteNonQuery();
        }

        public void Actualizar(Usuario usuario)
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            var query = @"
                UPDATE usuarios SET
                    nombre_completo = @NombreCompleto,
                    correo = @Correo,
                    telefono = @Telefono,
                    tipo = @Tipo,
                    estado = @Estado,
                    cargo = @Cargo,
                    departamento = @Departamento,
                    salario = @Salario,
                    horario = @Horario,
                    direccion = @Direccion,
                    ciudad = @Ciudad
                WHERE id = @Id";

            using var cmd = new MySqlCommand(query, connection);
            AgregarParametros(cmd, usuario);
            cmd.Parameters.AddWithValue("@Id", usuario.Id);
            cmd.ExecuteNonQuery();
        }

        public void Eliminar(int id)
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            var query = "DELETE FROM usuarios WHERE id = @Id";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        private void AgregarParametros(MySqlCommand cmd, Usuario usuario)
        {
            cmd.Parameters.AddWithValue("@NombreCompleto", usuario.NombreCompleto);
            cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
            cmd.Parameters.AddWithValue("@Telefono", usuario.Telefono);
            cmd.Parameters.AddWithValue("@Tipo", usuario.Tipo);
            cmd.Parameters.AddWithValue("@Estado", usuario.Estado);
            cmd.Parameters.AddWithValue("@FechaCreacion", usuario.FechaCreacion);

            // Empleado
            cmd.Parameters.AddWithValue("@Cargo", (object?)usuario.Cargo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Departamento", (object?)usuario.Departamento ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Salario", usuario.Salario);
            cmd.Parameters.AddWithValue("@Horario", (object?)usuario.Horario ?? DBNull.Value);

            // CCliente
            cmd.Parameters.AddWithValue("@Direccion", (object?)usuario.Direccion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Ciudad", (object?)usuario.Ciudad ?? DBNull.Value);
        }
    }
}
