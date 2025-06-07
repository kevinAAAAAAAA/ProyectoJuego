using System.Collections.Generic;
using UnityEngine;

using Assets._Project.Scripts.Gameplay;
using Assets._Project.Scripts.UI;
using Assets._Project.Scripts.Core;
namespace Assets._Project.Scripts.DSL
{

public class IfStatement : Statement
{
    public Expression Condition { get; }
    public Statement ThenBranch { get; } 
    public Statement ElseBranch { get; } 

    public IfStatement(Expression condition, Statement thenBranch, Statement elseBranch)
    {
        Condition = condition;
        ThenBranch = thenBranch;
        ElseBranch = elseBranch;
    }
}

public class BlockStatement : Statement
{
    public List<Statement> Statements { get; } = new List<Statement>();
    public BlockStatement(List<Statement> statements)
    {
        Statements = statements;
    }
}


public abstract class Statement {}

public class VariableDeclarationStatement : Statement
{
    public DslToken Name { get; }
    public Expression Initializer { get; } 
    public VariableDeclarationStatement(DslToken name, Expression initializer)
    {
        Name = name;
        Initializer = initializer;
    }
}

public class AssignmentStatement : Statement
{
    public DslToken Name { get; }
    public Expression Value { get; }
    public AssignmentStatement(DslToken name, Expression value)
    {
        Name = name;
        Value = value;
    }
}

public class GameCommandStatement : Statement
{
    public DslToken CommandName { get; }
    public List<Expression> Arguments { get; }
    public GameCommandStatement(DslToken commandName, List<Expression> arguments)
    {
        CommandName = commandName;
        Arguments = arguments;
    }
}

public abstract class Expression {}

public class LiteralExpression : Expression
{
    public object Value { get; }
    public LiteralExpression(object value) { Value = value; }
}

public class VariableExpression : Expression
{
    public DslToken Name { get; }
    public VariableExpression(DslToken name) { Name = name; }
}


public class DslParser
{
    private readonly List<DslToken> _tokens;
    private int _current = 0;
    public List<string> Errors { get; } = new List<string>();

    public DslParser(List<DslToken> tokens)
    {
        _tokens = tokens;
    }

    public List<Statement> Parse()
    {
        var statements = new List<Statement>();
        while (!IsAtEnd())
        {
            // Ignorar NEWLINEs vacíos entre sentencias
            while (Peek().Type == DslTokenType.NEWLINE && !IsAtEnd()) Advance();
            if (IsAtEnd()) break;

            try
            {
                statements.Add(ParseStatement());
            }
            catch (ParseError error)
            {
                Errors.Add(error.Message);
                Synchronize(); 
            }
        }
        return statements;
    }


    private Statement ParseStatement()
    {
        if (Match(DslTokenType.KEYWORD_VAR)) return ParseVariableDeclaration();
        if (Peek().Type == DslTokenType.IDENTIFIER && PeekNext().Type == DslTokenType.EQUALS)
        {
            return ParseAssignment();
        }
        if (Match(DslTokenType.KEYWORD_SI)) return ParseIfStatement(); 
        if (Peek().Type == DslTokenType.GAME_COMMAND || (Peek().Type == DslTokenType.IDENTIFIER && PeekNext()?.Type == DslTokenType.LPAREN))
        {
            return ParseGameCommandStatement();
        }
        if (Match(DslTokenType.KEYWORD_REPETIR)) return ParseRepeatStatement();

        throw new ParseError(Peek(), "Se esperaba una declaración, asignación, comando 'SI', 'REPETIR' o comando de juego.");
    }
    
    
    private Statement ParseRepeatStatement()
    {

        Expression timesExpression = ParseExpression(); 
        Consume(DslTokenType.KEYWORD_VECES, "Se esperaba 'VECES' después de la expresión numérica en 'REPETIR'.");

        Statement body = ParseBlockOrSingleStatementUntil(DslTokenType.KEYWORD_FINREPETIR);

        Consume(DslTokenType.KEYWORD_FINREPETIR, "Se esperaba 'FINREPETIR' para terminar el bloque 'REPETIR'.");

        return new RepeatStatement(timesExpression, body);
    }
    
    
    
    
        private Statement ParseIfStatement()
    {
        
        Consume(DslTokenType.LPAREN, "Se esperaba '(' después de 'SI'.");
        Expression condition = ParseExpression();
        Consume(DslTokenType.RPAREN, "Se esperaba ')' después de la condición del 'SI'.");
        Consume(DslTokenType.KEYWORD_ENTONCES, "Se esperaba 'ENTONCES' después de la condición.");

        Statement thenBranch = ParseBlockOrSingleStatementUntil(DslTokenType.KEYWORD_SINO, DslTokenType.KEYWORD_FINSI);

        Statement elseBranch = null;
        if (Match(DslTokenType.KEYWORD_SINO))
        {
            elseBranch = ParseBlockOrSingleStatementUntil(DslTokenType.KEYWORD_FINSI);
        }

        Consume(DslTokenType.KEYWORD_FINSI, "Se esperaba 'FINSI' para terminar el bloque 'SI'.");
        return new IfStatement(condition, thenBranch, elseBranch);
    }


    private Statement ParseBlockOrSingleStatementUntil(params DslTokenType[] terminators)
    {
        List<Statement> statements = new List<Statement>();
        while (!CheckTerminators(terminators) && !IsAtEnd())
        {
            while (Peek().Type == DslTokenType.NEWLINE && !IsAtEnd()) Advance();
            if (CheckTerminators(terminators) || IsAtEnd()) break;

            statements.Add(ParseStatement());
        }

        if (statements.Count == 1) return statements[0];
        if (statements.Count == 0)
        {
             return new BlockStatement(new List<Statement>());
        }
        return new BlockStatement(statements);
    }

    private bool CheckTerminators(DslTokenType[] terminators)
    {
        if (IsAtEnd()) return true; 
        foreach (var terminator in terminators)
        {
            if (Check(terminator)) return true;
        }
        return false;
    }


    private Expression ParseExpression()
    {
        if (Match(DslTokenType.LITERAL_TRUE)) return new LiteralExpression(true);
        if (Match(DslTokenType.LITERAL_FALSE)) return new LiteralExpression(false);

        if (Match(DslTokenType.NUMBER, DslTokenType.STRING))
        {
            return new LiteralExpression(Previous().Literal);
        }

        if (Check(DslTokenType.IDENTIFIER) || Check(DslTokenType.GAME_COMMAND))
        {
            DslToken token = Peek();
            if (token.Type == DslTokenType.GAME_COMMAND || (token.Type == DslTokenType.IDENTIFIER && PeekNext()?.Type == DslTokenType.LPAREN))
            {
                Advance(); 

                if (Match(DslTokenType.LPAREN))
                {
                    if (!Check(DslTokenType.RPAREN))
                    {
                    }
                    Consume(DslTokenType.RPAREN, $"Se esperaba ')' después de llamar a la función '{token.Lexeme}'.");
                    return new GameFunctionCallExpression(token);
                }
                else if (token.Type == DslTokenType.IDENTIFIER)
                {
                    return new VariableExpression(token);
                }
                else
                {
                    throw new ParseError(token, $"El comando '{token.Lexeme}' se usó incorrectamente en una expresión. ¿Faltó '()'? ");
                }
            }
            else if (token.Type == DslTokenType.IDENTIFIER) 
            {
                Advance(); 
                return new VariableExpression(token);
            }
        }

        throw new ParseError(Peek(), "Se esperaba una expresión (número, VERDADERO/FALSO, variable o llamada a función de condición).");
    }




    private Statement ParseVariableDeclaration()
    {
        DslToken name = Consume(DslTokenType.IDENTIFIER, "Se esperaba un nombre de variable después de 'VARIABLE'.");
        Consume(DslTokenType.EQUALS, "Se esperaba '=' después del nombre de variable.");
        Expression initializer = ParseExpression();
        return new VariableDeclarationStatement(name, initializer);
    }

    private Statement ParseAssignment()
    {
        DslToken name = Consume(DslTokenType.IDENTIFIER, "Se esperaba un nombre de variable para la asignación.");
        Consume(DslTokenType.EQUALS, "Se esperaba '=' después del nombre de variable en asignación.");
        Expression value = ParseExpression();
        return new AssignmentStatement(name, value);
    }

    private Statement ParseGameCommandStatement()
    {
        DslToken commandName = Advance(); 
        if (commandName.Type != DslTokenType.GAME_COMMAND && commandName.Type != DslTokenType.IDENTIFIER)
        {
            throw new ParseError(commandName, "Se esperaba un nombre de comando.");
        }

        Consume(DslTokenType.LPAREN, "Se esperaba '(' después del nombre del comando.");
        var arguments = new List<Expression>();
        if (!Check(DslTokenType.RPAREN))
        {
            do
            {
                if (arguments.Count >= 255) throw new ParseError(Peek(), "No pueden pasarse más de 255 argumentos.");
                arguments.Add(ParseExpression());
            } while (Match(DslTokenType.COMMA));
        }
        Consume(DslTokenType.RPAREN, "Se esperaba ')' después de los argumentos del comando.");
        return new GameCommandStatement(commandName, arguments);
    }




    private bool Match(params DslTokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    private DslToken Consume(DslTokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw new ParseError(Peek(), message);
    }

    private bool Check(DslTokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private DslToken Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private bool IsAtEnd() => Peek().Type == DslTokenType.EOF;
    private DslToken Peek() => _tokens[_current];
    private DslToken PeekNext()
    {
        if (_current + 1 >= _tokens.Count) return _tokens[_tokens.Count - 1]; 
        return _tokens[_current + 1];
    }
    private DslToken Previous() => _tokens[_current - 1];

    private void Synchronize() 
    {
        Advance();
        while (!IsAtEnd())
        {
            if (Previous().Type == DslTokenType.NEWLINE) return; 
            switch (Peek().Type)
            {
                case DslTokenType.KEYWORD_VAR:
                    return;
            }
            Advance();
        }
    }

    public class ParseError : System.Exception
    {
        public DslToken Token { get; }
        public ParseError(DslToken token, string message) : base($"Error en token '{token.Lexeme}' (línea {token.Line}): {message}")
        {
            Token = token;
        }
    }
}


public class GameFunctionCallExpression : Expression
{
    public DslToken FunctionName { get; }
    public GameFunctionCallExpression(DslToken functionName)
    {
        FunctionName = functionName;
    }
}



}