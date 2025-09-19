using ProyectoIntegradorS5.Modelos;
using ProyectoIntegradorS5.Views;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoIntegradorS5.Servicios { 

    //Esto será larguísimo, aviso desde ya, tengo estructurado esto ya, como vieron en las CRC, lo documentaré para no perderme pero yo hago esto entero
    // - Jonathan
public class SimplexService
{
        private double[,] tableau;
        private int filas;
        private int columnas;
        private List<string> variablesBasicas;
        private List<string> todasLasVariables; // NUEVA: lista completa de variables
        private bool esMaximizacion;

        public class ResultadoSimplex
    {
        public bool EsOptimo { get; set; }
        public bool EsIlimitado { get; set; }
        public bool EsInfactible { get; set; }
        public double ValorObjetivo { get; set; }
        public Dictionary<string, double> Solucion { get; set; } = new();
        public Dictionary<string, double> VariablesHolgura { get; set; } = new();
        public int Iteraciones { get; set; }
        public string MensajeError { get; set; } = "";
    }

    /// <param name="funcionObjetivo">Coeficientes de la función objetivo</param>
    /// <param name="restricciones">Matriz de coeficientes de restricciones</param>
    /// <param name="ladoDerecho">Valores del lado derecho de las restricciones</param>
    /// <param name="tiposRestriccion">Tipos de restricción (<=, =, >=)</param>
    /// <param name="nombresVariables">Nombres de las variables de decisión</param>
    /// <param name="esMaximizacion">True si es maximización, false si es minimización</param>
    public ResultadoSimplex Resolver(
        double[] funcionObjetivo,
        double[,] restricciones,
        double[] ladoDerecho,
        string[] tiposRestriccion,
        string[] nombresVariables,
        bool esMaximizacion)
    {
        try
        {
            this.esMaximizacion = esMaximizacion;

            // Validar entradas
            if (!ValidarEntradas(funcionObjetivo, restricciones, ladoDerecho, tiposRestriccion, nombresVariables))
            {
                return new ResultadoSimplex
                {
                    EsInfactible = true,
                    MensajeError = "Datos de entrada inválidos"
                };
            }

            // Convertir a forma estándar
            ConvertirAFormaEstandar(funcionObjetivo, restricciones, ladoDerecho, tiposRestriccion, nombresVariables);

            // Crear tableau inicial
            CrearTableauInicial();

            // Verificar factibilidad inicial
            if (!EsFactibleInicial())
            {
                return new ResultadoSimplex
                {
                    EsInfactible = true,
                    MensajeError = "Problema infactible - valores negativos en lado derecho"
                };
            }

            // Ejecutar algoritmo Simplex
            var resultado = EjecutarSimplex();

            return resultado;
        }
        catch (Exception ex)
        {
            return new ResultadoSimplex
            {
                EsInfactible = true,
                MensajeError = $"Error en el solver: {ex.Message}"
            };
        }
    }

    private bool ValidarEntradas(double[] funcionObjetivo, double[,] restricciones,
                               double[] ladoDerecho, string[] tiposRestriccion,
                               string[] nombresVariables)
    {
        if (funcionObjetivo == null || restricciones == null || ladoDerecho == null ||
            tiposRestriccion == null || nombresVariables == null)
            return false;

        int numVariables = funcionObjetivo.Length;
        int numRestricciones = ladoDerecho.Length;

        if (restricciones.GetLength(0) != numRestricciones ||
            restricciones.GetLength(1) != numVariables ||
            tiposRestriccion.Length != numRestricciones ||
            nombresVariables.Length != numVariables)
            return false;

        return true;
    }

        private void ConvertirAFormaEstandar(double[] funcionObjetivo, double[,] restricciones,
                                        double[] ladoDerecho, string[] tiposRestriccion,
                                        string[] nombresVariables)
        {
            int numVariables = funcionObjetivo.Length;
            int numRestricciones = ladoDerecho.Length;

            // Contar variables de holgura/exceso necesarias
            int variablesAdicionales = 0;
            for (int i = 0; i < numRestricciones; i++)
            {
                if (tiposRestriccion[i] == "<=" || tiposRestriccion[i] == ">=")
                    variablesAdicionales++;
            }

            // Dimensiones del nuevo problema
            int totalVariables = numVariables + variablesAdicionales;
            columnas = totalVariables + 1; // +1 para la columna del lado derecho
            filas = numRestricciones + 1;  // +1 para la fila de la función objetivo

            variablesBasicas = new List<string>();
            todasLasVariables = new List<string>();

            // Variables de decisión originales
            for (int i = 0; i < numVariables; i++)
            {
                todasLasVariables.Add(nombresVariables[i]);
            }

            // Crear tableau ampliado
            tableau = new double[filas, columnas];

            // Llenar función objetivo (primera fila)
            for (int j = 0; j < numVariables; j++)
            {
                tableau[0, j] = esMaximizacion ? -funcionObjetivo[j] : funcionObjetivo[j];
            }

            // Llenar restricciones y agregar variables de holgura/exceso
            int varAdicionalIndex = numVariables;

            for (int i = 0; i < numRestricciones; i++)
            {
                // Copiar coeficientes originales
                for (int j = 0; j < numVariables; j++)
                {
                    tableau[i + 1, j] = restricciones[i, j];
                }

                // Agregar variable de holgura o exceso
                if (tiposRestriccion[i] == "<=")
                {
                    tableau[i + 1, varAdicionalIndex] = 1; // Variable de holgura
                    string nombreHolgura = $"s{i + 1}";
                    todasLasVariables.Add(nombreHolgura);
                    variablesBasicas.Add(nombreHolgura); // Las variables de holgura inician como básicas
                    varAdicionalIndex++;
                }
                else if (tiposRestriccion[i] == ">=")
                {
                    tableau[i + 1, varAdicionalIndex] = -1; // Variable de exceso
                    string nombreExceso = $"e{i + 1}";
                    todasLasVariables.Add(nombreExceso);
                    variablesBasicas.Add(nombreExceso);
                    varAdicionalIndex++;
                }
                else if (tiposRestriccion[i] == "=")
                {
                    string nombreArtificial = $"a{i + 1}";
                    variablesBasicas.Add(nombreArtificial);
                    todasLasVariables.Add(nombreArtificial);
                }

                // Lado derecho
                tableau[i + 1, columnas - 1] = ladoDerecho[i];
            }
        }

        private void CrearTableauInicial()
    {
            // Deprecado, luego veo si lo reutilizo
    }

    private bool EsFactibleInicial()
    {
        // Verificar que todos los valores del lado derecho sean no negativos
        for (int i = 1; i < filas; i++)
        {
            if (tableau[i, columnas - 1] < -1e-10) // Tolerancia para errores numéricos
            {
                return false;
            }
        }
        return true;
    }
        // Lo feo
    private ResultadoSimplex EjecutarSimplex()
    {
        var resultado = new ResultadoSimplex();
        int iteraciones = 0;
        const int maxIteraciones = 1000;

            DebugTableauDetallado();

        while (iteraciones < maxIteraciones)
        {
            iteraciones++;

            // Paso 1: Encontrar variable entrante (columna pivote)
            int columnaPivote = EncontrarColumnaPivote();

            if (columnaPivote == -1)
            {
                // Solución óptima encontrada
                resultado.EsOptimo = true;
                break;
            }

            // Paso 2: Encontrar variable saliente (fila pivote)
            int filaPivote = EncontrarFilaPivote(columnaPivote);

            if (filaPivote == -1)
            {
                // Solución ilimitada
                resultado.EsIlimitado = true;
                resultado.MensajeError = "El problema tiene solución ilimitada";
                return resultado;
            }

            // Paso 3: Realizar operaciones de pivoteo
            RealizarPivoteo(filaPivote, columnaPivote);

            // Actualizar variables básicas y no básicas
            ActualizarVariables(filaPivote, columnaPivote);
        }

        if (iteraciones >= maxIteraciones)
        {
            resultado.MensajeError = "Se alcanzó el número máximo de iteraciones";
            return resultado;
        }

        resultado.Iteraciones = iteraciones;

        if (resultado.EsOptimo)
        {
            // Extraer solución
            ExtraerSolucion(resultado);
        }

            DebugTableauDetallado();


        return resultado;
    }


        public void DebugTableauDetallado()
        {
            System.Diagnostics.Debug.WriteLine("=== TABLEAU DETALLADO ===");

            // Encabezados
            System.Diagnostics.Debug.Write("Base\t\t");
            for (int j = 0; j < columnas - 1; j++)
            {
                if (j < todasLasVariables.Count)
                {
                    System.Diagnostics.Debug.Write($"{todasLasVariables[j]}\t");
                }
                else
                {
                    System.Diagnostics.Debug.Write($"Col{j}\t");
                }
            }
            System.Diagnostics.Debug.WriteLine("RHS");

            // Fila objetivo
            System.Diagnostics.Debug.Write("Z\t\t");
            for (int j = 0; j < columnas; j++)
            {
                System.Diagnostics.Debug.Write($"{tableau[0, j]:F2}\t");
            }
            System.Diagnostics.Debug.WriteLine("");

            // Filas de restricciones
            for (int i = 1; i < filas; i++)
            {
                if (i - 1 < variablesBasicas.Count)
                {
                    System.Diagnostics.Debug.Write($"{variablesBasicas[i - 1]}\t\t");
                }
                else
                {
                    System.Diagnostics.Debug.Write($"R{i}\t\t");
                }

                for (int j = 0; j < columnas; j++)
                {
                    System.Diagnostics.Debug.Write($"{tableau[i, j]:F2}\t");
                }
                System.Diagnostics.Debug.WriteLine("");
            }

            System.Diagnostics.Debug.WriteLine($"\nVariables básicas: {string.Join(", ", variablesBasicas)}");
            System.Diagnostics.Debug.WriteLine($"Todas las variables: {string.Join(", ", todasLasVariables)}");
            System.Diagnostics.Debug.WriteLine("========================");
        }
        private int EncontrarColumnaPivote()
        {
            int columnaPivote = -1;
            double valorMasNegativo = 0;

            // Para maximización: buscar el coeficiente MÁS NEGATIVO
            // Para minimización: buscar el coeficiente MÁS POSITIVO
            for (int j = 0; j < columnas - 1; j++)
            {
                if (esMaximizacion)
                {
                    // En maximización, continuar mientras haya valores negativos
                    if (tableau[0, j] < valorMasNegativo)
                    {
                        valorMasNegativo = tableau[0, j];
                        columnaPivote = j;
                    }
                }
                else
                {
                    // En minimización, continuar mientras haya valores positivos
                    if (tableau[0, j] > valorMasNegativo)
                    {
                        valorMasNegativo = tableau[0, j];
                        columnaPivote = j;
                    }
                }
            }

            return columnaPivote;
        }

        private int EncontrarFilaPivote(int columnaPivote)
    {
        int filaPivote = -1;
        double menorRatio = double.MaxValue;

        for (int i = 1; i < filas; i++)
        {
            double coeficiente = tableau[i, columnaPivote];
            double ladoDerecho = tableau[i, columnas - 1];

            // Solo considerar coeficientes positivos para evitar ratios negativos
            if (coeficiente > 1e-10)
            {
                double ratio = ladoDerecho / coeficiente;
                if (ratio >= 0 && ratio < menorRatio)
                {
                    menorRatio = ratio;
                    filaPivote = i;
                }
            }
        }

        return filaPivote;
    }

    private void RealizarPivoteo(int filaPivote, int columnaPivote)
    {
        double elementoPivote = tableau[filaPivote, columnaPivote];

        // Normalizar la fila pivote
        for (int j = 0; j < columnas; j++)
        {
            tableau[filaPivote, j] /= elementoPivote;
        }

        // Hacer ceros en la columna pivote
        for (int i = 0; i < filas; i++)
        {
            if (i != filaPivote)
            {
                double factor = tableau[i, columnaPivote];
                for (int j = 0; j < columnas; j++)
                {
                    tableau[i, j] -= factor * tableau[filaPivote, j];
                }
            }
        }
    }

        private void ActualizarVariables(int filaPivote, int columnaPivote)
        {
            // La variable que entra a la base
            string variableEntrante = todasLasVariables[columnaPivote];

            // La variable que sale de la base
            string variableSaliente = variablesBasicas[filaPivote - 1];

            // Intercambiar
            variablesBasicas[filaPivote - 1] = variableEntrante;
        }

        // CORRECCIÓN 15-09-25: Método mejorado para obtener nombre de variable
        private string ObtenerNombreVariable(int indice)
        {
            if (indice < todasLasVariables.Count)
                return todasLasVariables[indice];

            return $"x{indice + 1}";
        }

        private void ExtraerSolucion(ResultadoSimplex resultado)
        {
            // Valor de la función objetivo
            resultado.ValorObjetivo = esMaximizacion ? -tableau[0, columnas - 1] : tableau[0, columnas - 1];

            // Valores de las variables de decisión
            resultado.Solucion = new Dictionary<string, double>();
            resultado.VariablesHolgura = new Dictionary<string, double>();

            // Inicializar todas las variables de decisión en 0
            foreach (var variable in todasLasVariables)
            {
                if (!variable.StartsWith("s") && !variable.StartsWith("e") && !variable.StartsWith("a"))
                {
                    resultado.Solucion[variable] = 0;
                }
            }

            // Asignar valores a las variables básicas
            for (int i = 0; i < variablesBasicas.Count; i++)
            {
                string variable = variablesBasicas[i];
                double valor = tableau[i + 1, columnas - 1];

                if (variable.StartsWith("s") || variable.StartsWith("e"))
                {
                    resultado.VariablesHolgura[variable] = valor;
                }
                else if (!variable.StartsWith("a"))
                {
                    // Esta es una variable de decisión
                    resultado.Solucion[variable] = valor;
                }
            }
        }



        // Método de conveniencia para resolver problemas simples con restricciones 

        public ResultadoSimplex ResolverSimple(
    ObservableCollection<Producto_O> productos,
    ObservableCollection<Recurso_O> recursos,
    bool esMaximizacion = true)
        {
            try
            {
                if (productos.Count == 0 || recursos.Count == 0)
                {
                    return new ResultadoSimplex
                    {
                        EsInfactible = true,
                        MensajeError = "No hay productos o recursos definidos"
                    };
                }

                // Preparar datos para el solver
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

                // Resolver
                return Resolver(funcionObjetivo, restricciones, ladoDerecho,
                               tiposRestriccion, nombresVariables, esMaximizacion);
            }
            catch (Exception ex)
            {
                return new ResultadoSimplex
                {
                    EsInfactible = true,
                    MensajeError = $"Error al resolver: {ex.Message}"
                };
            }
        }

        private double ObtenerConsumoRecurso(Producto_O producto, int indiceRecurso)
        {
            return indiceRecurso switch
            {
                0 => producto.Recurso1,
                1 => producto.Recurso2,
                2 => producto.Recurso3,
                _ => 0
            };
        }


        // Realiza análisis de sensibilidad sobre la solución óptima

        public AnalisisSensibilidad AnalizarSensibilidad(ResultadoSimplex solucionOptima)
    {
        var analisis = new AnalisisSensibilidad();

        if (!solucionOptima.EsOptimo)
        {
            analisis.MensajeError = "No se puede realizar análisis de sensibilidad sin solución óptima";
            return analisis;
        }

        try
        {
            // Análisis de variables básicas y no básicas
            analisis.VariablesBasicas = new List<string>(variablesBasicas);
            analisis.VariablesNoBasicas = new List<string>(todasLasVariables);

            // Precios sombra (valores duales)
            analisis.PreciosSombra = CalcularPreciosSombra();

            // Rangos de optimalidad para coeficientes de función objetivo
            analisis.RangosOptimalidad = CalcularRangosOptimalidad();

            // Rangos de factibilidad para lado derecho
            analisis.RangosFactibilidad = CalcularRangosFactibilidad();

            return analisis;
        }
        catch (Exception ex)
        {
            analisis.MensajeError = $"Error en análisis de sensibilidad: {ex.Message}";
            return analisis;
        }
    }

    private Dictionary<string, double> CalcularPreciosSombra()
    {
        var precios = new Dictionary<string, double>();

        // Los precios sombra se encuentran en la fila 0 del tableau final
        // en las columnas correspondientes a las variables de holgura básicas
        for (int j = 0; j < columnas - 1; j++)
        {
            string nombreVariable = ObtenerNombreVariable(j);
            if (nombreVariable.StartsWith("s"))
            {
                precios[nombreVariable] = Math.Abs(tableau[0, j]);
            }
        }

        return precios;
    }

    private Dictionary<string, RangoOptimalidad> CalcularRangosOptimalidad()
    {
        var rangos = new Dictionary<string, RangoOptimalidad>();

        // Implementación simplificada
        // En una implementación completa, se calcularían los rangos exactos
        foreach (var variable in variablesBasicas.Concat(todasLasVariables))
        {
            if (!variable.StartsWith("s") && !variable.StartsWith("e") && !variable.StartsWith("a"))
            {
                rangos[variable] = new RangoOptimalidad
                {
                    LimiteInferior = double.NegativeInfinity,
                    LimiteSuperior = double.PositiveInfinity,
                    EsIlimitado = true
                };
            }
        }

        return rangos;
    }

    private Dictionary<string, RangoFactibilidad> CalcularRangosFactibilidad()
    {
        var rangos = new Dictionary<string, RangoFactibilidad>();

        // Implementación simplificada
        for (int i = 0; i < variablesBasicas.Count; i++)
        {
            string recurso = $"Recurso_{i + 1}";
            double valorActual = tableau[i + 1, columnas - 1];

            rangos[recurso] = new RangoFactibilidad
            {
                ValorActual = valorActual,
                LimiteInferior = Math.Max(0, valorActual - valorActual * 0.5),
                LimiteSuperior = valorActual + valorActual * 0.5,
                PrecioSombra = Math.Abs(tableau[0, columnas - 1])
            };
        }

        return rangos;
    }


    // Muestra el tableau actual (para debugging)
    public string MostrarTableau()
    {
        var resultado = new System.Text.StringBuilder();

        resultado.AppendLine("Tableau Simplex:");
        resultado.AppendLine(new string('-', 60));

        // Encabezados
        resultado.Append("Base\t");
        for (int j = 0; j < columnas - 1; j++)
        {
            resultado.Append($"x{j + 1}\t");
        }
        resultado.AppendLine("RHS");

        // Fila de función objetivo
        resultado.Append("Z\t");
        for (int j = 0; j < columnas; j++)
        {
            resultado.Append($"{tableau[0, j]:F2}\t");
        }
        resultado.AppendLine();

        // Filas de restricciones
        for (int i = 1; i < filas; i++)
        {
            if (i - 1 < variablesBasicas.Count)
            {
                resultado.Append($"{variablesBasicas[i - 1]}\t");
            }
            else
            {
                resultado.Append($"R{i}\t");
            }

            for (int j = 0; j < columnas; j++)
            {
                resultado.Append($"{tableau[i, j]:F2}\t");
            }
            resultado.AppendLine();
        }

        return resultado.ToString();
    }
}


// Clase para el análisis de sensibilidad

public class AnalisisSensibilidad
{
    public List<string> VariablesBasicas { get; set; } = new();
    public List<string> VariablesNoBasicas { get; set; } = new();
    public Dictionary<string, double> PreciosSombra { get; set; } = new();
    public Dictionary<string, RangoOptimalidad> RangosOptimalidad { get; set; } = new();
    public Dictionary<string, RangoFactibilidad> RangosFactibilidad { get; set; } = new();
    public string MensajeError { get; set; } = "";
}

public class RangoOptimalidad
{
    public double LimiteInferior { get; set; }
    public double LimiteSuperior { get; set; }
    public bool EsIlimitado { get; set; }
}

public class RangoFactibilidad
{
    public double ValorActual { get; set; }
    public double LimiteInferior { get; set; }
    public double LimiteSuperior { get; set; }
    public double PrecioSombra { get; set; }
}
}
