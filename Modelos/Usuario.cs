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
        public DateTime FechaNacimiento { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Rol { get; set; } // "Cliente" o "Empleado"
        public string Estado { get; set; } // "Activo" o "Inactivo"
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }

        // Opcional
        public string Direccion { get; set; }
        public string Ciudad { get; set; }

        // Solo empleado
        public string Cargo { get; set; }
        public string Departamento { get; set; }
        public decimal Salario { get; set; }
        public string Horario { get; set; }
    }

}
