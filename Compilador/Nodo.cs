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

    // Interface para el Visitor Semántico
    public interface IVisitorSemantico
    {
        void Visitar(NodoUsing nodo);
        void Visitar(NodoIf nodo);
        void Visitar(NodoReturn nodo);
        void Visitar(NodoExpresion nodo);
        void Visitar(NodoDeclaracion nodo);
        void Visitar(NodoAsignacion nodo);
    }

    // Nodo para la sentencia "using"
    public class NodoUsing : Nodo
    {
        public Nodo Expresion { get; set; }
        public Nodo Sentencia { get; set; }

        public override void Aceptar(IVisitorSemantico visitor)
        {
            visitor.Visitar(this);
        }
    }

    // Nodo para la sentencia "if" (con opción de "else")
    public class NodoIf : Nodo
    {
        public Nodo Condicion { get; set; }
        public Nodo SentenciaIf { get; set; }
        public Nodo SentenciaElse { get; set; } // Opcional

        public override void Aceptar(IVisitorSemantico visitor)
        {
            visitor.Visitar(this);
        }
    }

    // Nodo para la sentencia "return"
    public class NodoReturn : Nodo
    {
        public Nodo Expresion { get; set; }

        public override void Aceptar(IVisitorSemantico visitor)
        {
            visitor.Visitar(this);
        }
    }

    // Nodo para expresiones (por ejemplo, identificadores o literales)
    public class NodoExpresion : Nodo
    {
        public string Valor { get; set; } // Puede representar un identificador o literal

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
