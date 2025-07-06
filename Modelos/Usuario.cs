using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoIntegradorS5.Modelos
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Tipo { get; set; } // "Cliente" o "Empleado"
        public string Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaNacimiento { get; set; }

        // Empleado
        public string Cargo { get; set; }
        public string Departamento { get; set; }
        public decimal Salario { get; set; }
        public string Horario { get; set; }

        // Cliente
        public string Direccion { get; set; }
        public string Ciudad { get; set; }
    }
}
