using Assets._Project.Scripts.Gameplay;
using Assets._Project.Scripts.UI;
using Assets._Project.Scripts.Core;
namespace Assets._Project.Scripts.DSL
{



public class DslToken
{
    public DslTokenType Type { get; }
    public string Lexeme { get; }
    public object Literal { get; }
    public int Line { get; }

    public DslToken(DslTokenType type, string lexeme, object literal, int line)
    {
        Type = type;
        Lexeme = lexeme;
        Literal = literal;
        Line = line;
    }

    public override string ToString()
    {
        return $"{Type} {Lexeme} {Literal}";
    }
}

}