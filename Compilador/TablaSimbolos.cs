namespace Compilador
{
    public class TablaSimbolos
    {
        private Dictionary<string, string> tabla;

        public TablaSimbolos()
        {
            tabla = new Dictionary<string, string>();
        }

        // Método para agregar un símbolo
        public void Agregar(string nombre, string info)
        {
            if (!tabla.ContainsKey(nombre))
            {
                tabla[nombre] = info;
            }
        }

        // Método para buscar un símbolo
        public bool Existe(string nombre)
        {
            return tabla.ContainsKey(nombre);
        }
    }
}
