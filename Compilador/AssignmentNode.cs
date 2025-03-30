using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    // Nodo base
    public abstract class ASTNode { }

    // Declaración de sentencias
    public abstract class Statement : ASTNode { }

    // Nodo para asignación: identificador = expresión;
    public class AssignmentNode : Statement
    {
        public string Identifier { get; set; }
        public Expression Expr { get; set; }
        public AssignmentNode(string id, Expression expr)
        {
            Identifier = id;
            Expr = expr;
        }

        // Nodo para sentencias que son únicamente expresiones (p.ej. llamadas o evaluaciones)
        public class ExpressionStatementNode : Statement
        {
            public Expression Expr { get; set; }
            public ExpressionStatementNode(Expression expr)
            {
                Expr = expr;
            }
        }

        // Nodo para declaración de variable (por ejemplo: int x;)
        public class DeclarationNode : Statement
        {
            public string TypeKeyword { get; set; }
            public string Identifier { get; set; }

            public DeclarationNode(string typeKeyword, string identifier)
            {
                TypeKeyword = typeKeyword;
                Identifier = identifier;
            }
        }

    }

    // Declaración de expresiones
    public abstract class Expression : ASTNode { }

    // Expresión binaria (p.ej. 3 + 4)
    public class BinaryExpression : Expression
    {
        public Expression Left { get; set; }
        public Token Operator { get; set; }
        public Expression Right { get; set; }
        public BinaryExpression(Expression left, Token op, Expression right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
    }

    // Expresión numérica
    public class NumberExpression : Expression
    {
        public int Value { get; set; }
        public NumberExpression(int value)
        {
            Value = value;
        }
    }

    // Expresión de identificador
    public class IdentifierExpression : Expression
    {
        public string Name { get; set; }
        public IdentifierExpression(string name)
        {
            Name = name;
        }
    }
}
