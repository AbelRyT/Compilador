using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    public class GeneradorCodigoCpp
    {
        // Mapea tipos del lenguaje a tipos de C++
        private string MapearTipo(string tipo)
        {
            switch (tipo)
            {
                case "int":
                    return "int";
                case "string":
                    return "std::string";
                case "decimal":
                    return "double";
                case "DateTime":
                    return "std::string"; // Se puede ajustar según la implementación deseada
                default:
                    return tipo;
            }
        }

        // Método que traduce el código intermedio optimizado a C++ y retorna el código generado como un string.
        public string GenerarCodigoCpp(List<string> codigoIntermedioOptimizado)
        {
            StringBuilder cppCode = new StringBuilder();

            // Encabezados y estructura básica en C++
            cppCode.AppendLine("#include <iostream>");
            cppCode.AppendLine("#include <string>");
            cppCode.AppendLine("");
            cppCode.AppendLine("using namespace std;");
            cppCode.AppendLine("");
            cppCode.AppendLine("int main() {");

            // Recorrer cada línea del código intermedio
            foreach (string linea in codigoIntermedioOptimizado)
            {
                string trimmed = linea.Trim();

                // Si es una declaración: DECLARE <identificador> : <tipo>
                if (trimmed.StartsWith("DECLARE"))
                {
                    // Se asume el formato: DECLARE x : int
                    string[] partes = trimmed.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (partes.Length >= 4)
                    {
                        string identificador = partes[1];
                        string tipo = MapearTipo(partes[3]);
                        cppCode.AppendLine($"    {tipo} {identificador};");
                        continue;
                    }
                }
                // Si es una instrucción de retorno
                else if (trimmed.StartsWith("RETURN"))
                {
                    string valor = trimmed.Substring("RETURN".Length).Trim();
                    cppCode.AppendLine($"    return {valor};");
                    continue;
                }
                // Si es un salto incondicional (GOTO)
                else if (trimmed.StartsWith("GOTO"))
                {
                    cppCode.AppendLine($"    {trimmed};");
                    continue;
                }
                // Si es una etiqueta (por ejemplo, L1:)
                else if (trimmed.EndsWith(":"))
                {
                    cppCode.AppendLine(trimmed);
                    continue;
                }
                // Si es una instrucción condicional (IF_FALSE ...)
                else if (trimmed.StartsWith("IF_FALSE"))
                {
                    string[] partes = trimmed.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (partes.Length >= 4)
                    {
                        string cond = partes[1];
                        string etiqueta = partes[3];
                        cppCode.AppendLine($"    if (!({cond})) goto {etiqueta};");
                        continue;
                    }
                }
                // Si es una instrucción USING o END_USING
                else if (trimmed.StartsWith("USING"))
                {
                    cppCode.AppendLine($"    // {trimmed} // Sección USING");
                    continue;
                }
                else if (trimmed.StartsWith("END_USING"))
                {
                    cppCode.AppendLine($"    // {trimmed} // Fin sección USING");
                    continue;
                }
                // --- Nueva regla para Console.Wriline ---
                else if (trimmed.StartsWith("Console.Wriline"))
                {
                    // Se asume el formato: Console.Wriline(<contenido>);
                    int inicio = trimmed.IndexOf('(');
                    int fin = trimmed.LastIndexOf(')');
                    if (inicio != -1 && fin != -1 && fin > inicio)
                    {
                        string argumento = trimmed.Substring(inicio + 1, fin - inicio - 1).Trim();
                        // Se genera la instrucción de impresión en C++
                        cppCode.AppendLine($"    cout << {argumento} << endl;");
                        continue;
                    }
                }
                // Para otras instrucciones (asignaciones, expresiones, etc.)
                else
                {
                    if (!trimmed.EndsWith(";"))
                        trimmed += ";";
                    cppCode.AppendLine("    " + trimmed);
                }
            }

            cppCode.AppendLine("    return 0;");
            cppCode.AppendLine("}");

            return cppCode.ToString();
        }
    }
}
