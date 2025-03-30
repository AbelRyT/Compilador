using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    public class AnalizadorSintactico
    {
        private List<Token> tokens;
        private int currentIndex;

        // Lista para almacenar los errores sintácticos
        public List<string> Errores { get; private set; }

        // Lista que contendrá los nodos del AST generados dinámicamente
        public List<Nodo> AST { get; private set; }

        public AnalizadorSintactico(List<Token> tokens)
        {
            this.tokens = tokens;
            this.currentIndex = 0;
            Errores = new List<string>();
            AST = new List<Nodo>();
        }

        // Método principal de análisis sintáctico que genera el AST
        public void Parse()
        {
            while (currentIndex < tokens.Count && tokens[currentIndex].Tipo != TokenType.FinArchivo)
            {
                try
                {
                    Nodo nodo = ParseStatement();
                    if (nodo != null)
                        AST.Add(nodo);
                }
                catch (Exception ex)
                {
                    Errores.Add("Error sintáctico: " + ex.Message);
                    // En un parser robusto se implementaría una estrategia de recuperación
                    break;
                }
            }
        }

        // Determina qué tipo de sentencia se está analizando y retorna el nodo correspondiente
        private Nodo ParseStatement()
        {
            // Si el token actual es una palabra reservada que indica un tipo, asumimos que es una declaración
            if (IsTipoDeclaracion())
            {
                return ParseDeclaracion();
            }
            // Si el token actual es un identificador y el siguiente es "=", es una asignación
            if (Match(TokenType.Identificador) && LookAhead(1)?.Valor == "=")
            {
                return ParseAsignacion();
            }
            // Otras sentencias se pueden agregar aquí
            throw new Exception("Sentencia no reconocida en el contexto actual.");
        }

        // Verifica si el token actual es un tipo de dato (por ejemplo, "int", "string", etc.)
        private bool IsTipoDeclaracion()
        {
            Token token = tokens[currentIndex];
            return token.Tipo == TokenType.PalabraReservada && (token.Valor == "int" || token.Valor == "string" || token.Valor == "decimal" || token.Valor == "DateTime");
        }

        // Analiza una declaración de variable: por ejemplo, "int x;"
        private NodoDeclaracion ParseDeclaracion()
        {
            // Se asume que el primer token es el tipo
            string tipo = tokens[currentIndex].Valor;
            Consume(TokenType.PalabraReservada); // consume el tipo
            // Se espera un identificador
            string identificador = tokens[currentIndex].Valor;
            Consume(TokenType.Identificador);
            // Se espera el delimitador ";" al final
            Consume(TokenType.Delimitador, ";");
            return new NodoDeclaracion { Tipo = tipo, Identificador = identificador };
        }

        // Analiza una asignación: por ejemplo, "x = \"Hola\";"
        private NodoAsignacion ParseAsignacion()
        {
            string identificador = tokens[currentIndex].Valor;
            Consume(TokenType.Identificador);
            Consume(TokenType.Operador, "=");
            NodoExpresion exp = ParseExpresion();
            Consume(TokenType.Delimitador, ";");
            return new NodoAsignacion { Identificador = identificador, Expresion = exp };
        }

        // Analiza una expresión simple (en este ejemplo, se asume que es un literal o un identificador)
        private NodoExpresion ParseExpresion()
        {
            Token token = tokens[currentIndex];
            NodoExpresion nodo = new NodoExpresion();
            nodo.Valor = token.Valor;
            Advance();
            return nodo;
        }

        // Métodos auxiliares de consumo y avance de tokens
        private Token Consume(TokenType type, string value = null)
        {
            if (Match(type, value))
            {
                return Advance();
            }
            else
            {
                throw new Exception($"Se esperaba {type} '{value}' pero se encontró '{tokens[currentIndex].Valor}' en la línea {tokens[currentIndex].Linea}, columna {tokens[currentIndex].Columna}.");
            }
        }

        private bool Match(TokenType type, string value = null)
        {
            Token current = tokens[currentIndex];
            return current.Tipo == type && (value == null || current.Valor == value);
        }

        private Token Advance()
        {
            return tokens[currentIndex++];
        }

        private Token LookAhead(int offset)
        {
            int index = currentIndex + offset;
            return index < tokens.Count ? tokens[index] : null;
        }
    }
}
