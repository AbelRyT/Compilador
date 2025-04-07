using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    public abstract class Nodo
    {
        public abstract void Aceptar(IVisitorSemantico visitor);
    }

    // Nodo para representar el programa completo
    public class NodoPrograma : Nodo
    {
        public NodoNamespace Namespace { get; set; }
        public override void Aceptar(IVisitorSemantico visitor)
        {
            // Se podría implementar la visita para recorrer el AST completo
        }
    }

    // Nodo para la declaración de un namespace
    public class NodoNamespace : Nodo
    {
        public string Nombre { get; set; }
        public List<NodoClase> Clases { get; set; }
        public override void Aceptar(IVisitorSemantico visitor)
        {
            // Implementación de la visita si fuera necesaria
        }
    }

    // Nodo para la declaración de una clase
    public class NodoClase : Nodo
    {
        public string Nombre { get; set; }
        public List<NodoMetodo> Metodos { get; set; }
        public override void Aceptar(IVisitorSemantico visitor)
        {
            // Implementación de la visita si fuera necesaria
        }
    }

    // Nodo para la declaración de un método
    public class NodoMetodo : Nodo
    {
        public string Nombre { get; set; }
        public NodoBloque Cuerpo { get; set; }
        public override void Aceptar(IVisitorSemantico visitor)
        {
            // Implementación de la visita si fuera necesaria
        }
    }

    // Nodo para un bloque de instrucciones
    public class NodoBloque : Nodo
    {
        public List<Nodo> Instrucciones { get; set; }
        public override void Aceptar(IVisitorSemantico visitor)
        {
            foreach (var instruccion in Instrucciones)
            {
                instruccion.Aceptar(visitor);
            }
        }
    }

    // Nodo para el ciclo for
    public class NodoFor : Nodo
    {
        public Nodo Inicializacion { get; set; }
        public Nodo Condicion { get; set; }
        public Nodo Iteracion { get; set; }
        public NodoBloque Cuerpo { get; set; }
        public override void Aceptar(IVisitorSemantico visitor)
        {
            // Implementación de la visita si fuera necesaria
        }
    }

    // Nodo para invocar un método
    public class NodoLlamadaMetodo : Nodo
    {
        public string Nombre { get; set; }
        public List<Nodo> Argumentos { get; set; }
        public override void Aceptar(IVisitorSemantico visitor)
        {
            // Implementación de la visita si fuera necesaria
        }
    }

    // Los nodos ya existentes se mantienen:

    public interface IVisitorSemantico
    {
        void Visitar(NodoUsing nodo);
        void Visitar(NodoIf nodo);
        void Visitar(NodoReturn nodo);
        void Visitar(NodoExpresion nodo);
        void Visitar(NodoDeclaracion nodo);
        void Visitar(NodoAsignacion nodo);
    }

    public class NodoUsing : Nodo
    {
        public Nodo Expresion { get; set; }
        public Nodo Sentencia { get; set; }
        public override void Aceptar(IVisitorSemantico visitor)
        {
            visitor.Visitar(this);
        }
    }

    public class NodoIf : Nodo
    {
        public Nodo Condicion { get; set; }
        public Nodo SentenciaIf { get; set; }
        public Nodo SentenciaElse { get; set; }
        public override void Aceptar(IVisitorSemantico visitor)
        {
            visitor.Visitar(this);
        }
    }

    public class NodoReturn : Nodo
    {
        public Nodo Expresion { get; set; }
        public override void Aceptar(IVisitorSemantico visitor)
        {
            visitor.Visitar(this);
        }
    }

    public class NodoExpresion : Nodo
    {
        public string Valor { get; set; }
        public override void Aceptar(IVisitorSemantico visitor)
        {
            visitor.Visitar(this);
        }
    }

    public class NodoDeclaracion : Nodo
    {
        public string Tipo { get; set; }
        public string Identificador { get; set; }
        public override void Aceptar(IVisitorSemantico visitor)
        {
            visitor.Visitar(this);
        }
    }

    public class NodoAsignacion : Nodo
    {
        public string Identificador { get; set; }
        public Nodo Expresion { get; set; }
        public override void Aceptar(IVisitorSemantico visitor)
        {
            visitor.Visitar(this);
        }
    }

}
