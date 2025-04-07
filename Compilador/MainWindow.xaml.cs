using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;

namespace Compilador
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string archivoActual = string.Empty;
        private string proyectoActual = string.Empty;
        private Dictionary<string, string> archivosProyecto = new();
        public ObservableCollection<string> LexicalTokens { get; set; } = new();
        public ObservableCollection<string> SyntaxErrors { get; set; } = new();
        public ObservableCollection<string> SemanticErrors { get; set; } = new();
        public ObservableCollection<string> SymbolTable { get; set; } = new();
        public string TranslatedCode { get; set; } = string.Empty;
        public string IntermediateCode { get; set; } = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void txtCodigoFuente_TextChanged(object sender, TextChangedEventArgs e)
        {
            ActualizarNumerosDeLinea();
        }

        private void ActualizarNumerosDeLinea()
        {
            if (txtCodigoFuente == null) return;

            int totalLineas = txtCodigoFuente.LineCount;
            StringBuilder lineNumbers = new StringBuilder();

            for (int i = 1; i <= totalLineas; i++)
            {
                lineNumbers.AppendLine(i.ToString());
            }

            LineNumbers.Text = lineNumbers.ToString();
        }

        private void NuevoProyecto_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Selecciona la carpeta para el nuevo proyecto",
                Filter = "Carpeta|*.folder", // Truco para abrir un diálogo de carpeta.
                FileName = "Seleccionar_Carpeta"
            };

            if (dialog.ShowDialog() == true)
            {
                proyectoActual = Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
                archivosProyecto.Clear();
                ActualizarArbolDeProyecto();
            }
        }

        private void AbrirProyecto_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Seleccionar Proyecto",
                Filter = "Archivos CSharp (*.csharp)|*.csharp|Todos los archivos (*.*)|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                proyectoActual = Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
                archivosProyecto.Clear();

                var archivos = Directory.GetFiles(proyectoActual, "*.csharp", SearchOption.AllDirectories);
                foreach (var archivo in archivos)
                {
                    archivosProyecto[Path.GetFileName(archivo)] = archivo;
                }

                ActualizarArbolDeProyecto();
            }
        }

        private void ActualizarArbolDeProyecto()
        {
            ProjectTree.Items.Clear();

            foreach (var archivo in archivosProyecto)
            {
                var item = new TreeViewItem { Header = archivo.Key, Tag = archivo.Value };
                item.Selected += ArchivoSeleccionado;
                ProjectTree.Items.Add(item);
            }
        }

        private void ArchivoSeleccionado(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem item && item.Tag is string rutaArchivo)
            {
                archivoActual = rutaArchivo;
                txtCodigoFuente.Text = File.ReadAllText(archivoActual);
            }
        }

        private void GuardarArchivo_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(archivoActual))
            {
                GuardarComoArchivo_Click(sender, e);
            }
            else
            {
                File.WriteAllText(archivoActual, txtCodigoFuente.Text);
            }
        }

        private void GuardarComoArchivo_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Archivos CSharp (*.csharp)|*.csharp|Todos los archivos (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                archivoActual = saveFileDialog.FileName;
                File.WriteAllText(archivoActual, txtCodigoFuente.Text);

                if (!archivosProyecto.ContainsKey(Path.GetFileName(archivoActual)))
                {
                    archivosProyecto[Path.GetFileName(archivoActual)] = archivoActual;
                    ActualizarArbolDeProyecto();
                }
            }
        }

        private async void btnAnalizar_Click(object sender, RoutedEventArgs e)
        {
            string codigoFuente = txtCodigoFuente.Text;

            // --- Análisis Léxico (para visualizar tokens) ---
            AnalizadorLexico analizadorLexico = new AnalizadorLexico(codigoFuente);
            List<Token> tokens = new List<Token>();
            Token token;
            do
            {
                token = analizadorLexico.ObtenerSiguienteToken();
                tokens.Add(token);
            } while (token.Tipo != TokenType.FinArchivo);
            dgTokens.ItemsSource = tokens;

            string codigoCSharp = codigoFuente;
            EjecutarCodigo ejecutor = new EjecutarCodigo();
            string resultado = await ejecutor.EjecutarCodigoAsync(codigoCSharp);
            txtResultadoCpp.Text = resultado;

            AnalyzeAndTranslate(codigoFuente);

        }

        private void AnalyzeAndTranslate(string code)
        {
            LexicalTokens.Clear();
            SyntaxErrors.Clear();
            SemanticErrors.Clear();
            SymbolTable.Clear();
            TranslatedCode = string.Empty;
            IntermediateCode = string.Empty;

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();

            // Análisis Léxico
            foreach (var token in root.DescendantTokens())
            {
                LexicalTokens.Add($"Token -> '{token.Text}'");
            }

            // Análisis Sintáctico
            foreach (var diagnostic in syntaxTree.GetDiagnostics())
            {
                if (diagnostic.Severity == DiagnosticSeverity.Error)
                {
                    SyntaxErrors.Add(diagnostic.GetMessage());
                }
            }
            

            // Análisis Semántico
            var compilation = CSharpCompilation.Create("CodeAnalysis")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(syntaxTree);

            foreach (var diagnostic in compilation.GetDiagnostics())
            {
                if (diagnostic.Severity == DiagnosticSeverity.Error)
                {
                    SemanticErrors.Add(diagnostic.GetMessage());
                }
            }
            lbErroresSemanticos.ItemsSource = SemanticErrors;

            // Generar Tabla de Símbolos
            GenerateSymbolTable(root);
            lbTablaSimbolos.ItemsSource = SymbolTable;

            // Generar Código Intermedio
            GenerateIntermediateCode(root);

            // Traducción a C++
            txtCodigoCpp.Text = TranslateToCpp(root);
           // TranslatedCodeOutput.Text = TranslatedCode;
        }

        // Diccionario para mantener los valores calculados de las variables.
        private Dictionary<string, int> variableValues = new Dictionary<string, int>();

        // Método para evaluar expresiones constantes (soporta literales, identificadores y operaciones binarias básicas).
        private int EvaluateExpression(ExpressionSyntax expr, Dictionary<string, int> variables)
        {
            if (expr is LiteralExpressionSyntax literal)
            {
                // Se asume que el literal es entero.
                return int.Parse(literal.Token.ValueText);
            }
            else if (expr is IdentifierNameSyntax identifier)
            {
                string name = identifier.Identifier.Text;
                if (variables.TryGetValue(name, out int value))
                    return value;
                else
                    throw new Exception($"Variable '{name}' no definida.");
            }
            else if (expr is BinaryExpressionSyntax binaryExpr)
            {
                int left = EvaluateExpression(binaryExpr.Left, variables);
                int right = EvaluateExpression(binaryExpr.Right, variables);
                switch (binaryExpr.OperatorToken.Text)
                {
                    case "+": return left + right;
                    case "-": return left - right;
                    case "*": return left * right;
                    case "/": return right != 0 ? left / right : throw new DivideByZeroException();
                    default: throw new Exception($"Operador '{binaryExpr.OperatorToken.Text}' no soportado.");
                }
            }
            else
            {
                throw new Exception("Expresión no soportada para evaluación.");
            }
        }

        /// <summary>
        /// Genera la tabla de símbolos y actualiza los valores de las variables evaluando, en orden, declaraciones y asignaciones.
        /// </summary>
        private void GenerateSymbolTable(CompilationUnitSyntax root)
        {
            // Limpia la tabla de símbolos y el diccionario de variables.
            SymbolTable.Clear();
            variableValues.Clear();

            // Procesar declaraciones locales (por ejemplo, "int x = 0;")
            foreach (var localDecl in root.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
            {
                string tipo = localDecl.Declaration.Type.ToString().Trim();
                foreach (var variable in localDecl.Declaration.Variables)
                {
                    string nombre = variable.Identifier.Text;
                    int valor = 0;
                    if (variable.Initializer != null)
                    {
                        try
                        {
                            valor = EvaluateExpression(variable.Initializer.Value, variableValues);
                        }
                        catch (Exception ex)
                        {
                            // Si la expresión no es evaluable, se deja un valor por defecto o se registra el error.
                            valor = 0;
                        }
                    }
                    // Almacena el valor inicial en el diccionario.
                    variableValues[nombre] = valor;
                    SymbolTable.Add($"Nombre: {nombre}, Tipo: {tipo}, Valor: {valor}");
                }
            }

            // Procesar asignaciones (por ejemplo, "x = x + 5;")
            foreach (var assignExpr in root.DescendantNodes().OfType<AssignmentExpressionSyntax>())
            {
                // Se asume que el lado izquierdo es un identificador.
                string nombre = assignExpr.Left.ToString().Trim();
                try
                {
                    int valor = EvaluateExpression(assignExpr.Right, variableValues);
                    // Actualiza el valor en el diccionario.
                    variableValues[nombre] = valor;
                    // Actualiza la entrada en la tabla de símbolos (si existe).
                    for (int i = 0; i < SymbolTable.Count; i++)
                    {
                        if (SymbolTable[i].Contains($"Nombre: {nombre},"))
                        {
                            // Extraer el tipo de la cadena existente (o mantenerlo) y actualizar el valor.
                            // Para simplificar, se reconstruye la entrada.
                            string tipo = SymbolTable[i].Split(',')[1].Split(':')[1].Trim();
                            SymbolTable[i] = $"Nombre: {nombre}, Tipo: {tipo}, Valor: {valor}";
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Si no se puede evaluar la asignación, se puede registrar el error o ignorarlo.
                }
            }
        }



        private void GenerateIntermediateCode(CompilationUnitSyntax root)
        {
            StringBuilder intermediate = new StringBuilder();
            // Se usa Append para concatenar en una sola línea (o en pocas)
            intermediate.Append("// Código intermedio generado: ");

            // Procesar declaraciones globales (para programas de nivel superior)
            foreach (var globalStatement in root.Members.OfType<GlobalStatementSyntax>())
            {
                intermediate.Append("InicioGlobal: ");
                ProcessStatement(globalStatement.Statement, intermediate);
                intermediate.Append(" FinGlobal. ");
            }

            // Procesar declaraciones de campos en clases/estructuras
            foreach (var field in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
            {
                foreach (var variable in field.Declaration.Variables)
                {
                    intermediate.Append($"Campo: {field.Declaration.Type.ToString().Trim()} {variable.Identifier.Text}");
                    if (variable.Initializer != null)
                        intermediate.Append($" = {variable.Initializer.Value.ToString().Trim()}");
                    intermediate.Append(". ");
                }
            }

            // Procesar métodos
            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                intermediate.Append($"InicioMétodo {method.Identifier.Text}: ");
                if (method.Body != null)
                {
                    foreach (var statement in method.Body.Statements)
                    {
                        ProcessStatement(statement, intermediate);
                    }
                }
                else if (method.ExpressionBody != null)
                {
                    intermediate.Append($"Expresión: {method.ExpressionBody.Expression.ToString().Trim()}. ");
                }
                intermediate.Append("FinMétodo. ");
            }

            // Se asigna el código intermedio sin tantos saltos de línea.
            IntermediateCode = intermediate.ToString();
            // Se separa usando ". " como delimitador, eliminando líneas vacías
            lbCodigoOptimizado.ItemsSource = IntermediateCode.Split(new[] { ". " }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Procesa una sentencia y agrega su representación en español al StringBuilder.
        /// </summary>
        private void ProcessStatement(StatementSyntax statement, StringBuilder intermediate)
        {
            // Declaración local (por ejemplo: int x;)
            if (statement is LocalDeclarationStatementSyntax localDecl)
            {
                foreach (var variable in localDecl.Declaration.Variables)
                {
                    intermediate.Append($"Declarar {localDecl.Declaration.Type.ToString().Trim()} {variable.Identifier.Text}");
                    if (variable.Initializer != null)
                        intermediate.Append($" = {variable.Initializer.Value.ToString().Trim()}");
                    intermediate.Append(". ");
                }
            }
            // Expresión (por ejemplo: x = x + 5 + 2;)
            else if (statement is ExpressionStatementSyntax expressionStmt)
            {
                if (expressionStmt.Expression is AssignmentExpressionSyntax assignExpr)
                {
                    intermediate.Append($"Asignar: {assignExpr.Left.ToString().Trim()} = {assignExpr.Right.ToString().Trim()}. ");
                }
                else
                {
                    intermediate.Append($"{statement.Kind()} : {statement.ToString().Trim()}. ");
                }
            }
            // Otros tipos de sentencias se procesan de forma genérica
            else
            {
                intermediate.Append($"{statement.Kind()} : {statement.ToString().Trim()}. ");
            }
        }


        /// <summary>
        /// Traduce una unidad de compilación de C# a código C++.
        /// Recorre namespaces, clases y métodos.
        /// </summary>
        private string TranslateToCpp(CompilationUnitSyntax root)
        {
            StringBuilder cppCode = new StringBuilder();
            cppCode.AppendLine("#include <iostream>");
            cppCode.AppendLine("#include <string>");
            cppCode.AppendLine("using namespace std;\n");

            // Procesar clases y namespaces (si existen)
            if (root.Members.Any(m => m is NamespaceDeclarationSyntax))
            {
                foreach (var ns in root.Members.OfType<NamespaceDeclarationSyntax>())
                {
                    foreach (var classDecl in ns.Members.OfType<ClassDeclarationSyntax>())
                    {
                        cppCode.AppendLine(TranslateClassToCpp(classDecl));
                    }
                }
            }
            else
            {
                foreach (var classDecl in root.Members.OfType<ClassDeclarationSyntax>())
                {
                    cppCode.AppendLine(TranslateClassToCpp(classDecl));
                }
            }

            // Procesar sentencias globales (top-level statements)
            var globalStatements = root.Members.OfType<GlobalStatementSyntax>();
            if (globalStatements.Any())
            {
                cppCode.AppendLine("int main() {");
                foreach (var globalStmt in globalStatements)
                {
                    cppCode.AppendLine(TranslateStatementToCpp(globalStmt.Statement));
                }
                cppCode.AppendLine("    return 0;");
                cppCode.AppendLine("}");
            }

            return cppCode.ToString();
        }


        /// <summary>
        /// Traduce una clase de C# a código C++.
        /// </summary>
        private string TranslateClassToCpp(ClassDeclarationSyntax classDecl)
        {
            StringBuilder classCode = new StringBuilder();
            classCode.AppendLine($"class {classDecl.Identifier.Text} {{");
            classCode.AppendLine("public:");
            foreach (var member in classDecl.Members)
            {
                if (member is MethodDeclarationSyntax methodDecl)
                {
                    classCode.AppendLine(TranslateMethodToCpp(methodDecl));
                }
                // Aquí se pueden agregar traducciones para campos u otros miembros si se requiere
            }
            classCode.AppendLine("};\n");
            return classCode.ToString();
        }

        /// <summary>
        /// Traduce un método de C# a código C++.
        /// Incluye la conversión de parámetros, tipo de retorno y cuerpo del método.
        /// </summary>
        private string TranslateMethodToCpp(MethodDeclarationSyntax methodDecl)
        {
            string returnType = MapType(methodDecl.ReturnType.ToString());
            string methodName = methodDecl.Identifier.Text;
            string parameters = string.Join(", ", methodDecl.ParameterList.Parameters.Select(p =>
                MapType(p.Type.ToString()) + " " + p.Identifier.Text));

            StringBuilder methodCode = new StringBuilder();
            methodCode.AppendLine($"    {returnType} {methodName}({parameters}) {{");

            // Si el método tiene cuerpo
            if (methodDecl.Body != null)
            {
                foreach (var statement in methodDecl.Body.Statements)
                {
                    methodCode.AppendLine(TranslateStatementToCpp(statement));
                }
            }
            // Si utiliza cuerpo de expresión (=>)
            else if (methodDecl.ExpressionBody != null)
            {
                methodCode.AppendLine("        return " + TranslateExpressionToCpp(methodDecl.ExpressionBody.Expression) + ";");
            }
            methodCode.AppendLine("    }");
            return methodCode.ToString();
        }

        /// <summary>
        /// Traduce sentencias de C# a C++.
        /// Se implementa el manejo de declaraciones locales, asignaciones, return, Console.WriteLine e if.
        /// Otros casos se pueden agregar según se requiera.
        /// </summary>
        private string TranslateStatementToCpp(StatementSyntax statement)
        {
            // Manejo de sentencias if
            if (statement is IfStatementSyntax ifStmt)
            {
                StringBuilder code = new StringBuilder();
                code.Append("        if (");
                code.Append(TranslateExpressionToCpp(ifStmt.Condition));
                code.AppendLine(") {");
                // Procesa el bloque del if o la sentencia única
                if (ifStmt.Statement is BlockSyntax block)
                {
                    foreach (var stmt in block.Statements)
                    {
                        code.AppendLine(TranslateStatementToCpp(stmt));
                    }
                }
                else
                {
                    code.AppendLine(TranslateStatementToCpp(ifStmt.Statement));
                }
                code.Append("        }");
                // Manejo de la cláusula else, si existe
                if (ifStmt.Else != null)
                {
                    code.AppendLine(" else {");
                    if (ifStmt.Else.Statement is BlockSyntax elseBlock)
                    {
                        foreach (var stmt in elseBlock.Statements)
                        {
                            code.AppendLine(TranslateStatementToCpp(stmt));
                        }
                    }
                    else
                    {
                        code.AppendLine(TranslateStatementToCpp(ifStmt.Else.Statement));
                    }
                    code.Append("        }");
                }
                return code.ToString();
            }
            // Declaración local (por ejemplo: int x;)
            else if (statement is LocalDeclarationStatementSyntax localDecl)
            {
                StringBuilder line = new StringBuilder("        ");
                string tipoCpp = MapType(localDecl.Declaration.Type.ToString());
                foreach (var variable in localDecl.Declaration.Variables)
                {
                    line.Append($"{tipoCpp} {variable.Identifier.Text}");
                    if (variable.Initializer != null)
                    {
                        line.Append(" = " + TranslateExpressionToCpp(variable.Initializer.Value));
                    }
                    line.Append(";");
                }
                return line.ToString();
            }
            // Expresión: puede ser llamada a Console.WriteLine o asignación
            else if (statement is ExpressionStatementSyntax expressionStmt)
            {
                // Caso especial: traducir Console.WriteLine(a, b, ...)
                if (expressionStmt.Expression is InvocationExpressionSyntax invocationExpr)
                {
                    if (invocationExpr.Expression is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Expression.ToString().Trim() == "Console" &&
                        memberAccess.Name.ToString().Trim() == "WriteLine")
                    {
                        StringBuilder line = new StringBuilder("        cout << ");
                        var args = invocationExpr.ArgumentList.Arguments;
                        bool first = true;
                        foreach (var arg in args)
                        {
                            if (!first)
                                line.Append(" << ");
                            line.Append(TranslateExpressionToCpp(arg.Expression));
                            first = false;
                        }
                        line.Append(" << endl;");
                        return line.ToString();
                    }
                }
                // Asignación (por ejemplo: x = x + 5;)
                if (expressionStmt.Expression is AssignmentExpressionSyntax assignExpr)
                {
                    string left = assignExpr.Left.ToString().Trim();
                    string right = TranslateExpressionToCpp(assignExpr.Right);
                    return $"        {left} = {right};";
                }
                return "        // [Expresión no traducida]";
            }
            // Sentencia return
            else if (statement is ReturnStatementSyntax returnStmt)
            {
                return "        return " + TranslateExpressionToCpp(returnStmt.Expression) + ";";
            }
            // Caso genérico: sentencia no reconocida
            return "        // [Sentencia no traducida]";
        }



        /// <summary>
        /// Traduce expresiones básicas de C# a C++.
        /// Se manejan expresiones binarias, literales e identificadores.
        /// </summary>
        private string TranslateExpressionToCpp(ExpressionSyntax expr)
        {
            if (expr is BinaryExpressionSyntax binaryExpr)
            {
                string left = TranslateExpressionToCpp(binaryExpr.Left);
                string right = TranslateExpressionToCpp(binaryExpr.Right);
                string op = binaryExpr.OperatorToken.Text;
                return $"{left} {op} {right}";
            }
            else if (expr is LiteralExpressionSyntax literal)
            {
                return literal.Token.Text;
            }
            else if (expr is IdentifierNameSyntax identifier)
            {
                return identifier.Identifier.Text;
            }
            // Caso por defecto: se utiliza el ToString() del nodo
            return expr.ToString();
        }

        /// <summary>
        /// Mapeo básico de tipos de C# a C++.
        /// Se pueden ampliar los casos según la complejidad del código a traducir.
        /// </summary>
        private string MapType(string csType)
        {
            return csType switch
            {
                "int" => "int",
                "float" => "float",
                "double" => "double",
                "bool" => "bool",
                "string" => "std::string",
                "void" => "void",
                _ => csType,
            };
        }

    }
}