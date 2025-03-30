using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Compilador
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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

            // --- Análisis Sintáctico ---
            AnalizadorSintactico parser = new AnalizadorSintactico(tokens);
            parser.Parse();
            lbErroresSintacticos.ItemsSource = parser.Errores;

            // --- Análisis Semántico ---
            // Se procesa cada nodo del AST generado dinámicamente por el parser
            AnalizadorSemantico analizadorSemantico = new AnalizadorSemantico();
            foreach (var nodo in parser.AST)
            {
                nodo.Aceptar(analizadorSemantico);
            }
            lbErroresSemanticos.ItemsSource = analizadorSemantico.Errores;

            // --- Generación de Código Intermedio ---
            GeneradorCodigoIntermedio generador = new GeneradorCodigoIntermedio();
            foreach (var nodo in parser.AST)
            {
                nodo.Aceptar(generador);
            }

            // --- Optimización del Código Intermedio ---
            OptimizadorCodigoIntermedio optimizador = new OptimizadorCodigoIntermedio();
            List<string> codigoOpt = optimizador.Optimizar(generador.CodigoIntermedio);
            lbCodigoOptimizado.ItemsSource = codigoOpt;

            GeneradorCodigoCpp generadorCpp = new GeneradorCodigoCpp();
            string codigoCpp = generadorCpp.GenerarCodigoCpp(codigoOpt);
            txtCodigoCpp.Text = codigoCpp;

            string codigoCSharp = codigoFuente;
            EjecutarCodigo ejecutor = new EjecutarCodigo();
            string resultado = await ejecutor.EjecutarCodigoAsync(codigoCSharp);
            txtResultadoCpp.Text = resultado;

        }
    }
}