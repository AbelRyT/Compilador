namespace Compilador
{
    public class OptimizadorCodigoIntermedio
    {
        // Método para optimizar una lista de instrucciones del código intermedio
        public List<string> Optimizar(List<string> codigoIntermedio)
        {
            List<string> codigoOpt = new List<string>();
            Dictionary<string, string> constantes = new Dictionary<string, string>();

            // Paso 1: Constant Folding y Propagación
            foreach (var linea in codigoIntermedio)
            {
                string lineaOptimizada = linea;

                // Si la línea es una asignación simple (por ejemplo: t1 = 3)
                if (linea.Contains("="))
                {
                    var partes = linea.Split(new char[] { '=' }, 2);
                    if (partes.Length == 2)
                    {
                        string lhs = partes[0].Trim();
                        string rhs = partes[1].Trim();

                        // Si el lado derecho es un literal (número o cadena), lo registramos
                        if (EsConstante(rhs))
                        {
                            if (!constantes.ContainsKey(lhs))
                                constantes.Add(lhs, rhs);
                            else
                                constantes[lhs] = rhs;
                        }
                    }
                }

                // Reemplazar en la línea el uso de temporales que tengan un valor constante
                foreach (var kvp in constantes)
                {
                    if (lineaOptimizada.Contains(kvp.Key))
                    {
                        lineaOptimizada = lineaOptimizada.Replace(kvp.Key, kvp.Value);
                    }
                }

                codigoOpt.Add(lineaOptimizada);
            }

            // Paso 2: Eliminación de saltos redundantes
            List<string> codigoFinal = new List<string>();
            for (int i = 0; i < codigoOpt.Count; i++)
            {
                string lineaActual = codigoOpt[i].Trim();
                // Si la línea comienza con "GOTO"
                if (lineaActual.StartsWith("GOTO"))
                {
                    string[] partesGoto = lineaActual.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (partesGoto.Length == 2)
                    {
                        string etiqueta = partesGoto[1];
                        // Si la siguiente línea es la etiqueta de destino (por ejemplo, "L1:"), se elimina el GOTO
                        if (i + 1 < codigoOpt.Count && codigoOpt[i + 1].Trim() == etiqueta + ":")
                        {
                            continue;
                        }
                    }
                }
                codigoFinal.Add(codigoOpt[i]);
            }

            return codigoFinal;
        }

        // Método auxiliar para determinar si un valor es una constante (literal numérico o cadena)
        private bool EsConstante(string valor)
        {
            valor = valor.Trim();
            // Si es una cadena (entre comillas)
            if (valor.StartsWith("\"") && valor.EndsWith("\""))
                return true;
            // Si es un número entero
            int num;
            if (int.TryParse(valor, out num))
                return true;
            return false;
        }
    }
}
