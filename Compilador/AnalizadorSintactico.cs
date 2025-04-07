using System;
using System.CodeDom;
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
        }

        public void Parse()
        {
            var programa = ParseProgram();
            AST = new List<Nodo> { programa };
        }

        // Método principal: analiza todo el programa
        private NodoPrograma ParseProgram()
        {
            NodoPrograma programa = new NodoPrograma();
            // Procesa directivas using (se pueden omitir en el análisis posterior)
            while (Match(TokenType.PalabraReservada, "using"))
            {
                // Se descarta la directiva hasta encontrar el delimitador ;
                while (!Match(TokenType.Delimitador, ";"))
                {
                    Advance();
                }
                Consume(TokenType.Delimitador, ";");
            }
            // Se procesa la declaración de namespace
            NodoNamespace nodoNamespace = ParseNamespace();
            programa.Namespace = nodoNamespace;
            return programa;
        }

        private NodoNamespace ParseNamespace()
        {
            Consume(TokenType.PalabraReservada, "namespace");
            string nombre = Consume(TokenType.Identificador).Valor;
            Consume(TokenType.Delimitador, "{");
            NodoNamespace nodoNamespace = new NodoNamespace();
            nodoNamespace.Nombre = nombre;
            nodoNamespace.Clases = new List<NodoClase>();

            while (!Match(TokenType.Delimitador, "}"))
            {
                NodoClase clase = ParseClass();
                if (clase != null)
                    nodoNamespace.Clases.Add(clase);
            }
            Consume(TokenType.Delimitador, "}");
            return nodoNamespace;
        }

        private NodoClase ParseClass()
        {
            Consume(TokenType.PalabraReservada, "class");
            string nombreClase = Consume(TokenType.Identificador).Valor;
            Consume(TokenType.Delimitador, "{");
            NodoClase nodoClase = new NodoClase();
            nodoClase.Nombre = nombreClase;
            nodoClase.Metodos = new List<NodoMetodo>();

            while (!Match(TokenType.Delimitador, "}"))
            {
                NodoMetodo metodo = ParseMethod();
                if (metodo != null)
                    nodoClase.Metodos.Add(metodo);
            }
            Consume(TokenType.Delimitador, "}");
            return nodoClase;
        }

        private NodoMetodo ParseMethod()
        {
            // Para simplificar se omiten modificadores y parámetros
            // Se asume que la firma es similar a: static void Main(string[] args)
            while (!Match(TokenType.Identificador))
            {
                Advance();
            }
            string nombreMetodo = Consume(TokenType.Identificador).Valor;
            Consume(TokenType.Delimitador, "(");
            // Ignorar parámetros (se puede ampliar según necesidad)
            while (!Match(TokenType.Delimitador, ")"))
            {
                Advance();
            }
            Consume(TokenType.Delimitador, ")");
            NodoMetodo nodoMetodo = new NodoMetodo();
            nodoMetodo.Nombre = nombreMetodo;
            nodoMetodo.Cuerpo = ParseBlock();
            return nodoMetodo;
        }

        private NodoBloque ParseBlock()
        {
            Consume(TokenType.Delimitador, "{");
            NodoBloque bloque = new NodoBloque();
            bloque.Instrucciones = new List<Nodo>();

            while (!Match(TokenType.Delimitador, "}"))
            {
                Nodo instruccion = ParseStatement();
                if (instruccion != null)
                    bloque.Instrucciones.Add(instruccion);
            }
            Consume(TokenType.Delimitador, "}");
            return bloque;
        }

        private Nodo ParseStatement()
        {
            // Sentencia if
            if (Match(TokenType.PalabraReservada, "if"))
                return ParseIf();

            // Sentencia for
            if (Match(TokenType.PalabraReservada, "for"))
                return ParseFor();

            // Declaración de variable (con inicialización opcional)
            if (IsTipoDeclaracion())
                return ParseDeclaracionConInicializacion();

            // Asignación o llamada a método
            if (Match(TokenType.Identificador))
            {
                Token siguiente = LookAhead(1);
                if (siguiente != null && siguiente.Valor == "=")
                    return ParseAsignacion();
                else if (siguiente != null && siguiente.Valor == "(")
                    return ParseLlamadaMetodo();
            }

            throw new Exception("Sentencia no reconocida en el contexto actual.");
        }

        private NodoIf ParseIf()
        {
            Consume(TokenType.PalabraReservada, "if");
            Consume(TokenType.Delimitador, "(");
            Nodo condicion = ParseExpresionCompleja();
            Consume(TokenType.Delimitador, ")");
            Nodo sentenciaIf = ParseStatement();
            Nodo sentenciaElse = null;
            if (Match(TokenType.PalabraReservada, "else"))
            {
                Consume(TokenType.PalabraReservada, "else");
                sentenciaElse = ParseStatement();
            }
            NodoIf nodoIf = new NodoIf();
            nodoIf.Condicion = condicion;
            nodoIf.SentenciaIf = sentenciaIf;
            nodoIf.SentenciaElse = sentenciaElse;
            return nodoIf;
        }

        private NodoFor ParseFor()
        {
            Consume(TokenType.PalabraReservada, "for");
            Consume(TokenType.Delimitador, "(");
            // Se asume que la inicialización es una declaración o asignación
            Nodo inicializacion = ParseStatement();
            Nodo condicion = ParseExpresionCompleja();
            Consume(TokenType.Delimitador, ";");
            Nodo iteracion = ParseExpresionCompleja();
            Consume(TokenType.Delimitador, ")");
            NodoBloque cuerpo = ParseBlock();

            NodoFor nodoFor = new NodoFor();
            nodoFor.Inicializacion = inicializacion;
            nodoFor.Condicion = condicion;
            nodoFor.Iteracion = iteracion;
            nodoFor.Cuerpo = cuerpo;
            return nodoFor;
        }

        // Soporta declaraciones con inicialización, por ejemplo: int resultado = Sumar(5, 3);
        private Nodo ParseDeclaracionConInicializacion()
        {
            string tipo = tokens[currentIndex].Valor;
            Consume(TokenType.PalabraReservada); // consume el tipo
            string identificador = Consume(TokenType.Identificador).Valor;
            if (Match(TokenType.Operador, "="))
            {
                Consume(TokenType.Operador, "=");
                NodoExpresion expr = (NodoExpresion)ParseExpresionCompleja();
                Consume(TokenType.Delimitador, ";");
                // Se utiliza un nodo de asignación para representar la inicialización
                return new NodoAsignacion { Identificador = identificador, Expresion = expr };
            }
            Consume(TokenType.Delimitador, ";");
            return new NodoDeclaracion { Tipo = tipo, Identificador = identificador };
        }

        // Una expresión compleja puede ser una invocación de método o una expresión simple
        private Nodo ParseExpresionCompleja()
        {
            if (Match(TokenType.Identificador) && LookAhead(1)?.Valor == "(")
                return ParseLlamadaMetodo();
            return ParseExpresion();
        }

        private NodoExpresion ParseExpresion()
        {
            Token token = tokens[currentIndex];
            NodoExpresion nodo = new NodoExpresion();
            nodo.Valor = token.Valor;
            Advance();
            return nodo;
        }

        private NodoAsignacion ParseAsignacion()
        {
            string identificador = Consume(TokenType.Identificador).Valor;
            Consume(TokenType.Operador, "=");
            NodoExpresion expr = (NodoExpresion)ParseExpresionCompleja();
            Consume(TokenType.Delimitador, ";");
            return new NodoAsignacion { Identificador = identificador, Expresion = expr };
        }

        private NodoLlamadaMetodo ParseLlamadaMetodo()
        {
            string nombreMetodo = Consume(TokenType.Identificador).Valor;
            Consume(TokenType.Delimitador, "(");
            List<Nodo> argumentos = new List<Nodo>();
            if (!Match(TokenType.Delimitador, ")"))
            {
                argumentos.Add(ParseExpresionCompleja());
                while (Match(TokenType.Delimitador, ","))
                {
                    Consume(TokenType.Delimitador, ",");
                    argumentos.Add(ParseExpresionCompleja());
                }
            }
            Consume(TokenType.Delimitador, ")");
            Consume(TokenType.Delimitador, ";");
            NodoLlamadaMetodo llamada = new NodoLlamadaMetodo();
            llamada.Nombre = nombreMetodo;
            llamada.Argumentos = argumentos;
            return llamada;
        }

        private bool IsTipoDeclaracion()
        {
            Token token = tokens[currentIndex];
            return token.Tipo == TokenType.PalabraReservada &&
                   (token.Valor == "int" || token.Valor == "string" ||
                    token.Valor == "decimal" || token.Valor == "DateTime");
        }

        private Token Consume(TokenType type, string value = null)
        {
            if (Match(type, value))
                return Advance();
            else
                throw new Exception($"Se esperaba {type} '{value}' pero se encontró '{tokens[currentIndex].Valor}' en la línea {tokens[currentIndex].Linea}, columna {tokens[currentIndex].Columna}.");
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
