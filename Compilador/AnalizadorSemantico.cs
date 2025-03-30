using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    public class AnalizadorSemantico : IVisitorSemantico
    {
        // Tabla de símbolos: clave es el nombre de la variable, valor es el tipo declarado.
        private Dictionary<string, string> tablaSimbolos;
        public List<string> Errores { get; private set; }

        public AnalizadorSemantico()
        {
            tablaSimbolos = new Dictionary<string, string>();
            Errores = new List<string>();
        }

        public void Analizar(Nodo nodo)
        {
            nodo.Aceptar(this);
        }

        // Visita para nodo de declaración
        public void Visitar(NodoDeclaracion nodo)
        {
            if (!tablaSimbolos.ContainsKey(nodo.Identificador))
            {
                tablaSimbolos.Add(nodo.Identificador, nodo.Tipo);
            }
            else
            {
                Errores.Add($"Error semántico: La variable '{nodo.Identificador}' ya ha sido declarada.");
            }
        }

        // Visita para nodo de asignación
        public void Visitar(NodoAsignacion nodo)
        {
            if (!tablaSimbolos.ContainsKey(nodo.Identificador))
            {
                Errores.Add($"Error semántico: La variable '{nodo.Identificador}' no ha sido declarada.");
                return;
            }
            string tipoDeclarado = tablaSimbolos[nodo.Identificador];
            string tipoExpresion = EvaluarTipo(nodo.Expresion);

            if (tipoDeclarado != tipoExpresion)
            {
                Errores.Add($"Error semántico: Tipo incompatible en la asignación de '{nodo.Identificador}'. Se esperaba '{tipoDeclarado}', pero se obtuvo '{tipoExpresion}'.");
            }
        }

        // Los métodos existentes se mantienen sin cambios...
        public void Visitar(NodoUsing nodo)
        {
            if (nodo.Expresion != null)
                nodo.Expresion.Aceptar(this);
            if (nodo.Sentencia != null)
                nodo.Sentencia.Aceptar(this);
        }

        public void Visitar(NodoIf nodo)
        {
            if (nodo.Condicion != null)
                nodo.Condicion.Aceptar(this);
            if (nodo.SentenciaIf != null)
                nodo.SentenciaIf.Aceptar(this);
            if (nodo.SentenciaElse != null)
                nodo.SentenciaElse.Aceptar(this);
        }

        public void Visitar(NodoReturn nodo)
        {
            if (nodo.Expresion != null)
                nodo.Expresion.Aceptar(this);
        }

        public void Visitar(NodoExpresion nodo)
        {
            // Para identificar el tipo del literal:
            // Si el valor está entre comillas, se considera string.
            if (!string.IsNullOrEmpty(nodo.Valor))
            {
                // Este método solo se usa para evaluar literales en asignaciones.
            }
        }

        // Método auxiliar para determinar el tipo de una expresión simple.
        private string EvaluarTipo(Nodo nodo)
        {
            if (nodo is NodoExpresion exp)
            {
                if (!string.IsNullOrEmpty(exp.Valor))
                {
                    // Si la cadena comienza y termina con comillas, es un string.
                    if (exp.Valor.StartsWith("\"") && exp.Valor.EndsWith("\""))
                        return "string";
                    // Intentar parsear a entero
                    else if (int.TryParse(exp.Valor, out _))
                        return "int";
                    // Aquí se pueden agregar más casos (decimal, bool, etc.)
                }
            }
            return "unknown";
        }
    }
}
