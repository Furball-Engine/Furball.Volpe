using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Furball.Volpe.Evaluation;
using Furball.Volpe.Exceptions;

namespace Furball.Volpe.LexicalAnalysis; 

public class Lexer
{
    private readonly TextConsumer _textConsumer;

    public Lexer(string source)
    {
        _textConsumer = new TextConsumer(source);
    }

    private string ConsumeNextString()
    {
        char          character;
        StringBuilder stringBuilder = new StringBuilder();
            
        if (!_textConsumer.TryConsumeNext(out character) || character != '"')
            throw new InvalidOperationException();

        for(;;)
        {
            if (!_textConsumer.TryConsumeNext(out character))
                throw new UnexpectedEofException(_textConsumer.PositionInText);

            switch (character)
            {
                case '"':
                    return stringBuilder.ToString();
                    
                default:
                    stringBuilder.Append(character);
                    break;
            }
        }
    }

    private string ConsumeNextLiteral()
    {
        StringBuilder stringBuilder = new StringBuilder();
            
        for(;;)
        {
            if (!_textConsumer.TryPeekNext(out char character))
                break;
                
            if (character.CanBeInVolpeLiteral())
            {
                stringBuilder.Append(character);
                _textConsumer.SkipOne();
            } 
            else 
                break;
        }

        return stringBuilder.ToString();
    }

    private (double? asDouble, byte? asByte) ConsumeNextNumber()
    {
        double num        = 0;
        int    exp        = 0;
        bool   isRational = false;
        bool   isByte     = false;
            
        for(;;)
        {
            if (!_textConsumer.TryPeekNext(out char character))
                break;

            if (character.IsVolpeDigit())
            {
                //There should be no characters after `b` in a byte
                if(isByte)
                    throw new UnexceptedSymbolException(character, _textConsumer.PositionInText);
                
                num = num * 10 + (character - '0');

                if (isRational)
                    exp--;
                    
                _textConsumer.TryConsumeNext(out _);
            } 
            else if (character == '.')
            {
                if (isRational || isByte)
                    throw new UnexceptedSymbolException('.', _textConsumer.PositionInText);
                        
                isRational = true;

                _textConsumer.TryConsumeNext(out _);
            } 
            else if (character == 'b') {
                if(isByte || isRational)
                    throw new UnexceptedSymbolException('b', _textConsumer.PositionInText);
                
                isByte = true;
                
                _textConsumer.TryConsumeNext(out _);
            }
            else
                break;
        }

        (double? asDouble, byte? asByte) val = new();

        if (isByte) {
            if (num is > 255 or < 0)
                throw new OutOfBoundsException(num.ToString(CultureInfo.InvariantCulture), typeof(byte), this._textConsumer.PositionInText);
            
            val.asByte = (byte)num;
        }
        else
            val.asDouble = Math.Pow(10, exp) * num;
        
        return val;
    }

    private void SkipWhiteSpaces()
    {
        while (_textConsumer.TryPeekNext(out char character) && char.IsWhiteSpace(character))
            _textConsumer.TryConsumeNext(out _);
    }

    private TokenValue ConsumeOperator()
    {
        StringBuilder stringBuilder = new StringBuilder();
            
        for(;;)
        {
            if (!_textConsumer.TryPeekNext(out char character))
                break;
                
            if (character.IsVolpeOperator())
            {
                stringBuilder.Append(character);
                _textConsumer.SkipOne();
            } 
            else 
                break; 
        }

        string op = stringBuilder.ToString();
        return op switch
        {
            "="  => new TokenValue.Assign(),
            "++" => new TokenValue.Append(),
            "==" => new TokenValue.BooleanOperator(new TokenValueBooleanOperator.Eq()),
            ">"  => new TokenValue.BooleanOperator(new TokenValueBooleanOperator.GreaterThan()),
            "->" => new TokenValue.ClassAccess(),
            "<"  => new TokenValue.BooleanOperator(new TokenValueBooleanOperator.LessThan()),
            ">=" => new TokenValue.BooleanOperator(new TokenValueBooleanOperator.GreaterThanOrEqual()),
            "<=" => new TokenValue.BooleanOperator(new TokenValueBooleanOperator.LessThanOrEqual()),
            "&&" => new TokenValue.BooleanOperator(new TokenValueBooleanOperator.And()),
            "||" => new TokenValue.BooleanOperator(new TokenValueBooleanOperator.Or()),
            "!=" => new TokenValue.BooleanOperator(new TokenValueBooleanOperator.NotEq()),
            "!"  => new TokenValue.BooleanOperator(new TokenValueBooleanOperator.Not()),
                
            _ when op.EndsWith("=") => 
                new TokenValue.OperatorWithAssignment(TokenValueOperator.FromCharacter(op[0])),
                
            _ when op.Length == 1 => new TokenValue.ArithmeticalOperator(TokenValueOperator.FromCharacter(op[0])),
                
            _ => throw new InvalidOperatorStringException(op, _textConsumer.PositionInText),
        };
    }

    public bool TryConsumeNextToken(out Token? token)
    {
        token = null;
            
        SkipWhiteSpaces();
        PositionInText currentPositionInText = _textConsumer.PositionInText;

        char character;
        if (!_textConsumer.TryPeekNext(out character))
            return false;

        TokenValue tokenValue = null;
        switch (character) {
            case '"':
                tokenValue = new TokenValue.String(this.ConsumeNextString());
                break;
            case ':':
                tokenValue = this._textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.Colon());
                break;
            case ',':
                tokenValue = this._textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.Comma());
                break;
            case '#':
                tokenValue = this._textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.Hashtag());
                break;
            case '[':
                tokenValue = this._textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.LeftBracket());
                break;
            case ']':
                tokenValue = this._textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.RightBracket());
                break;
            case '(':
                tokenValue = this._textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.LeftRoundBracket());
                break;
            case ')':
                tokenValue = this._textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.RightRoundBracket());
                break;
            case '{':
                tokenValue = this._textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.LeftCurlyBracket());
                break;
            case '}':
                tokenValue = this._textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.RightCurlyBracket());
                break;
            case '$':
                tokenValue = this._textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.Dollar());
                break;
            case ';':
                tokenValue = this._textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.SemiColon());
                break;
            case '.':
                tokenValue = this._textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.Dot());
                break;
            case var _ when character.IsVolpeOperator():
                tokenValue = this.ConsumeOperator();
                break;
            case var _ when character.IsVolpeDigit():
                (double? asDouble, byte? asByte) val = this.ConsumeNextNumber();

                if (val.asByte.HasValue)
                    tokenValue = new TokenValue.Byte(val.asByte.Value);
                else if(val.asDouble.HasValue)
                    tokenValue = new TokenValue.Number(val.asDouble.Value);
                break;
            case var _ when character.CanBeInVolpeLiteral():
                tokenValue = this.ConsumeNextLiteral() switch {
                    "true"    => new TokenValue.True(),
                    "false"   => new TokenValue.False(),
                    "funcdef" => new TokenValue.FuncDef(),
                    "func"    => new TokenValue.Func(),
                    "ret"     => new TokenValue.Return(),
                    "if"      => new TokenValue.If(),
                    "elif"    => new TokenValue.Elif(),
                    "else"    => new TokenValue.Else(),
                    "while"   => new TokenValue.While(),
                    "class"   => new TokenValue.Class(),
                    "extends" => new TokenValue.Extends(),

                    {} value => new TokenValue.Literal(value)
                };
                break;
            case var c:
                throw new UnexceptedSymbolException(c, currentPositionInText);
        }

        token = new Token {Value = tokenValue, PositionInText = currentPositionInText};

        return true;
    }

    public IEnumerable<Token> GetTokenEnumerator()
    {
        Token? token;
        while (TryConsumeNextToken(out token))
            yield return token!;
    }
}