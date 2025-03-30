using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    public class EjecutarCodigo
    {
        public async Task<string> EjecutarCodigoAsync(string codigo)
        {
            // Redirige la salida estándar a un StringWriter para capturar el output
            StringWriter sw = new StringWriter();
            TextWriter originalOut = Console.Out;
            Console.SetOut(sw);

            try
            {
                // Ejecuta el código (se puede ajustar ScriptOptions según se requiera)
                await CSharpScript.RunAsync(codigo, ScriptOptions.Default.WithImports("System"));
            }
            catch (Exception ex)
            {
                sw.WriteLine("Error al ejecutar el código: " + ex.Message);
            }
            finally
            {
                // Restaura la salida original
                Console.SetOut(originalOut);
            }

            return sw.ToString();
        }
    }
}
