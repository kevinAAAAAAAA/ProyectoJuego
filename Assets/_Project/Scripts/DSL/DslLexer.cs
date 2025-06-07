using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine; 

using Assets._Project.Scripts.Gameplay;
using Assets._Project.Scripts.UI;
using Assets._Project.Scripts.Core;
namespace Assets._Project.Scripts.DSL
{




public class DslLexer
{
    private readonly string _source;
    private readonly List<DslToken> _tokens = new List<DslToken>();
    private int _start = 0;
    private int _current = 0;
    private int _line = 1;
    private static readonly Dictionary<string, DslTokenType> KeywordsAndCommands = new Dictionary<string, DslTokenType>(System.StringComparer.OrdinalIgnoreCase)
    {
        {"VARIABLE", DslTokenType.KEYWORD_VAR},
        {"SI", DslTokenType.KEYWORD_SI},
        {"ENTONCES", DslTokenType.KEYWORD_ENTONCES},
        {"SINO", DslTokenType.KEYWORD_SINO},
        {"FINSI", DslTokenType.KEYWORD_FINSI},
        {"VERDADERO", DslTokenType.LITERAL_TRUE},
        {"FALSO", DslTokenType.LITERAL_FALSE},


        {"REPETIR", DslTokenType.KEYWORD_REPETIR},
        {"VECES", DslTokenType.KEYWORD_VECES},
        {"FINREPETIR", DslTokenType.KEYWORD_FINREPETIR},


        {"MOVER_ADELANTE", DslTokenType.GAME_COMMAND},
        {"GIRAR_IZQUIERDA", DslTokenType.GAME_COMMAND},
        {"GIRAR_DERECHA", DslTokenType.GAME_COMMAND},
        {"HAY_PARED_ADELANTE", DslTokenType.GAME_COMMAND},


        {"RECOGER_LLAVE", DslTokenType.GAME_COMMAND},
        {"TIENE_LLAVE", DslTokenType.GAME_COMMAND},
        {"ABRIR_PUERTA", DslTokenType.GAME_COMMAND},
    };
    private static readonly Dictionary<string, DslTokenType> Keywords = new Dictionary<string, DslTokenType>
    {
        {"VARIABLE", DslTokenType.KEYWORD_VAR}
    };


    public DslLexer(string source)
    {
        _source = source;
    }

    public List<DslToken> ScanTokens()
    {
        _tokens.Clear();
        _current = 0;
        _start = 0;
        _line = 1;

        while (!IsAtEnd())
        {
            _start = _current;
            ScanNextToken();
        }
        _tokens.Add(new DslToken(DslTokenType.EOF, "", null, _line));
        return _tokens;
    }

    private void ScanNextToken()
    {
        char c = _source[_current]; 

        if (char.IsWhiteSpace(c))
        {
            if (c == '\n')
            {
                _line++;
            }
            _current++;
            return;
        }

        if (c == '/' && _current + 1 < _source.Length && _source[_current + 1] == '/')
        {
            while (_current < _source.Length && _source[_current] != '\n') _current++;
            return;
        }

        switch (c)
        {
            case '=': AddTokenAndAdvance(DslTokenType.EQUALS); return;
            case '(': AddTokenAndAdvance(DslTokenType.LPAREN); return;
            case ')': AddTokenAndAdvance(DslTokenType.RPAREN); return;
            case ',': AddTokenAndAdvance(DslTokenType.COMMA); return;
        }

        if (char.IsDigit(c))
        {
            ScanNumber();
            return;
        }

        if (c == '"')
        {
            ScanString();
            return;
        }

        if (IsAlpha(c)) 
        {
            ScanIdentifierOrKeyword();
            return;
        }



        AddToken(DslTokenType.ERROR, _source[_current].ToString(), null);
        Debug.LogError($"Lexer Error: Unexpected character '{_source[_current]}' at line {_line}");
        _current++; 
    }

    private void AddTokenAndAdvance(DslTokenType type)
    {
        AddToken(type, _source[_current].ToString(), null);
        _current++;
    }

    private void ScanNumber()
    {
        int start = _current;
        while (_current < _source.Length && char.IsDigit(_source[_current])) _current++;

        if (_current < _source.Length && _source[_current] == '.' &&
            _current + 1 < _source.Length && char.IsDigit(_source[_current + 1]))
        {
            _current++; 
            while (_current < _source.Length && char.IsDigit(_source[_current])) _current++;
            string lexeme = _source.Substring(start, _current - start);
            AddToken(DslTokenType.NUMBER, lexeme, float.Parse(lexeme, System.Globalization.CultureInfo.InvariantCulture));
        }
        else
        {
            string lexeme = _source.Substring(start, _current - start);
            AddToken(DslTokenType.NUMBER, lexeme, int.Parse(lexeme, System.Globalization.CultureInfo.InvariantCulture));
        }
    }
     private void ScanString()
    {
        _current++;
        int start = _current;
        while (_current < _source.Length && _source[_current] != '"')
        {
            if (_source[_current] == '\n') _line++; 
            _current++;
        }

        if (_current >= _source.Length)
        {
            AddToken(DslTokenType.ERROR, "Unterminated string.", null);
            return;
        }

        _current++;
        string value = _source.Substring(start, _current - start -1);
        AddToken(DslTokenType.STRING, "\"" + value + "\"", value);
    }


    private void ScanIdentifierOrKeyword()
    {
        int start = _current;
        while (_current < _source.Length && IsAlphaNumeric(_source[_current])) _current++;

        string text = _source.Substring(start, _current - start);
        if (KeywordsAndCommands.TryGetValue(text, out DslTokenType type))
        {
            if (type == DslTokenType.LITERAL_TRUE) AddToken(type, text, true);
            else if (type == DslTokenType.LITERAL_FALSE) AddToken(type, text, false);
            else AddToken(type, text, null);
        }
        else
        {
            AddToken(DslTokenType.IDENTIFIER, text, null);
        }
    }

    private bool IsAlpha(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
    private bool IsAlphaNumeric(char c) => IsAlpha(c) || char.IsDigit(c);

    private void AddToken(DslTokenType type, string lexeme, object literal)
    {
        _tokens.Add(new DslToken(type, lexeme, literal, _line));
    }
    private bool IsAtEnd() => _current >= _source.Length;









}

}