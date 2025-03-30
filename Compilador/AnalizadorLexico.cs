namespace Compilador
{
    public class AnalizadorLexico
    {
        private string codigoFuente;
        private int indice;
        private int linea;
        private int columna;
        private TablaSimbolos tablaSimbolos;

        public AnalizadorLexico(string codigo)
        {
            codigoFuente = codigo;
            indice = 0;
            linea = 1;
            columna = 1;
            tablaSimbolos = new TablaSimbolos();
        }

        // Método principal para obtener el siguiente token
        public Token ObtenerSiguienteToken()
        {
            // Saltar espacios y actualizar líneas/columnas
            while (indice < codigoFuente.Length && char.IsWhiteSpace(codigoFuente[indice]))
            {
                if (codigoFuente[indice] == '\n')
                {
                    linea++;
                    columna = 1;
                }
                else
                {
                    columna++;
                }
                indice++;
            }

            if (indice >= codigoFuente.Length)
                return new Token { Tipo = TokenType.FinArchivo, Valor = "EOF", Linea = linea, Columna = columna };

            char actual = codigoFuente[indice];

            // Caso: Identificadores o palabras reservadas (también permiten guion bajo)
            if (char.IsLetter(actual) || actual == '_')
            {
                int inicio = indice;
                int colInicio = columna;
                while (indice < codigoFuente.Length && (char.IsLetterOrDigit(codigoFuente[indice]) || codigoFuente[indice] == '_'))
                {
                    indice++;
                    columna++;
                }
                string lexema = codigoFuente.Substring(inicio, indice - inicio);
                if (EsPalabraReservada(lexema))
                {
                    return new Token { Tipo = TokenType.PalabraReservada, Valor = lexema, Linea = linea, Columna = colInicio };
                }
                else
                {
                    // Se agrega a la tabla de símbolos si es necesario
                    if (!tablaSimbolos.Existe(lexema))
                    {
                        tablaSimbolos.Agregar(lexema, "Identificador");
                    }
                    return new Token { Tipo = TokenType.Identificador, Valor = lexema, Linea = linea, Columna = colInicio };
                }
            }

            // Caso: Números (literal numérico)
            if (char.IsDigit(actual))
            {
                int inicio = indice;
                int colInicio = columna;
                while (indice < codigoFuente.Length && char.IsDigit(codigoFuente[indice]))
                {
                    indice++;
                    columna++;
                }
                // Se podría ampliar para soportar decimales
                string lexema = codigoFuente.Substring(inicio, indice - inicio);
                return new Token { Tipo = TokenType.Numero, Valor = lexema, Linea = linea, Columna = colInicio };
            }

            // Caso: Literales de cadena (entre comillas dobles)
            if (actual == '"')
            {
                int colInicio = columna;
                indice++; // Saltar comilla de apertura
                columna++;
                int inicio = indice;
                while (indice < codigoFuente.Length && codigoFuente[indice] != '"')
                {
                    // Manejo simple de caracteres de escape
                    if (codigoFuente[indice] == '\\' && indice + 1 < codigoFuente.Length)
                    {
                        indice++;
                        columna++;
                    }
                    indice++;
                    columna++;
                }
                if (indice < codigoFuente.Length)
                {
                    string lexema = codigoFuente.Substring(inicio, indice - inicio);
                    indice++; // Saltar comilla de cierre
                    columna++;
                    return new Token { Tipo = TokenType.Cadena, Valor = lexema, Linea = linea, Columna = colInicio };
                }
                else
                {
                    return new Token { Tipo = TokenType.Error, Valor = "Cadena no cerrada", Linea = linea, Columna = colInicio };
                }
            }

            // Caso: Símbolos y operadores (delimitadores, paréntesis, etc.)
            int colToken = columna;
            switch (actual)
            {
                case '(':
                case ')':
                case '{':
                case '}':
                case '[':
                case ']':
                case ';':
                case '.':
                case ',':
                case ':':
                    indice++;
                    columna++;
                    return new Token { Tipo = TokenType.Delimitador, Valor = actual.ToString(), Linea = linea, Columna = colToken };

                case '=':
                    indice++;
                    columna++;
                    // Se puede extender para detectar "==" u otros operadores compuestos
                    return new Token { Tipo = TokenType.Operador, Valor = "=", Linea = linea, Columna = colToken };

                case '+':
                    indice++;
                    columna++;
                    return new Token { Tipo = TokenType.Operador, Valor = "+", Linea = linea, Columna = colToken };

                case '-':
                    indice++;
                    columna++;
                    return new Token { Tipo = TokenType.Operador, Valor = "-", Linea = linea, Columna = colToken };

                case '*':
                    indice++;
                    columna++;
                    return new Token { Tipo = TokenType.Operador, Valor = "*", Linea = linea, Columna = colToken };

                case '/':
                    indice++;
                    columna++;
                    return new Token { Tipo = TokenType.Operador, Valor = "/", Linea = linea, Columna = colToken };

                // Aquí se pueden agregar más casos para otros operadores (+, -, *, /, etc.)
                default:
                    // Si el carácter no es reconocido, se marca como error
                    indice++;
                    columna++;
                    return new Token { Tipo = TokenType.Error, Valor = actual.ToString(), Linea = linea, Columna = colToken };
            }
        }

        // Método auxiliar para identificar palabras reservadas
        private bool EsPalabraReservada(string lexema)
        {
            // Lista simplificada de palabras reservadas en C#
            string[] reservadas = { "if", "else", "while", "for", "class", "public", "private", "void", "int", "string", "decimal"};
            return Array.Exists(reservadas, palabra => palabra.Equals(lexema, StringComparison.OrdinalIgnoreCase));
        }
    }
}
